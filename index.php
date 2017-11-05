<!DOCTYPE html>	
<html>
	<head>
		<meta charset="UTF-8">
		<title>Matrix</title>
		<meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
		<link rel="stylesheet" href="index.css" />
	</head>
	<body>
		<input id="searchBox" type="text" placeholder="Search" />
		<div id="permaLink"><a id="permaLinkAnchorHref">PL</a></div>
		<div id="map"></div>
		<!--<div id="signsCopyGert">Signs &copy; by Gert</div>-->
		<script>
			function loadJson(document, callback) {
				var xobj = new XMLHttpRequest();
				xobj.overrideMimeType("application/json");
				xobj.open('GET', document, true);
				xobj.onreadystatechange = function () {
					if (xobj.readyState == 4 && xobj.status == "200") {
						callback(xobj.responseText);
					}
				};
				xobj.send(null);  
			}

			function getInfoWindowContent (roadWays) {
				var iwContent = '';

				roadWays.forEach (function (roadWay) {
					iwContent = iwContent + roadWay.hmLocation + '<br />';
					
					roadWay.lanes.forEach(function (lane) {
						debugger;
						var shownSign = 'MSI-afbeeldingen/leeg.png';
						var laneNumber = 'Rijstrook ' + lane.number;
						if (lane.number == 'DRIP') {
							shownSign = '';
							laneNumber = lane.number;
						}

						iwContent = iwContent + '<img src="' + shownSign + '" title="' + laneNumber + '" id="' + lane.uuid + '" />&nbsp;';
					});
					iwContent = iwContent + '<br />';
				});
				
				return iwContent;
			}

			var map;
			var matrixPortalPoints = new Map();
			var liveMatrixBorden = [];
			function initMap() {
				var mapValues = {
					lat: parseFloat(getParamByName('lat') || 52.3393958),
					lon: parseFloat(getParamByName('lon') || 5.3028748),
					zoom: parseFloat(getParamByName('zoom') || 9)
				};
				
				map = new google.maps.Map(document.getElementById('map'), {
					zoom: mapValues.zoom,
					center: new google.maps.LatLng(mapValues.lat, mapValues.lon),
					mapTypeId: 'hybrid'
				});

				var searchBoxInput = document.getElementById('searchBox');
				var searchBox = new google.maps.places.SearchBox(searchBoxInput);
				map.controls[google.maps.ControlPosition.TOP_LEFT].push(searchBoxInput);
				
				var signsCopyGertDiv = document.getElementById('signsCopyGert');
				map.controls[google.maps.ControlPosition.TOP_LEFT].push(signsCopyGertDiv);

				var permaLinkDiv = document.getElementById('permaLink');
				map.controls[google.maps.ControlPosition.RIGHT_BOTTOM].push(permaLinkDiv);

				// Bias the SearchBox results towards current map's viewport.
				map.addListener('bounds_changed', function() {
					searchBox.setBounds(map.getBounds());
				});

				var markers = [];
				// Listen for the event fired when the user selects a prediction and retrieve
				// more details for that place.
				searchBox.addListener('places_changed', function() {
					var places = searchBox.getPlaces();

					if (places.length == 0) {
						return;
					}

					// Clear out the old markers.
					markers.forEach(function(marker) {
						marker.setMap(null);
					});
					markers = [];

					// For each place, get the icon, name and location.
					var bounds = new google.maps.LatLngBounds();
					places.forEach(function(place) {
						if (!place.geometry) {
							console.log("Returned place contains no geometry");
							return;
						}
						var icon = {
							url: place.icon,
							size: new google.maps.Size(71, 71),
							origin: new google.maps.Point(0, 0),
							anchor: new google.maps.Point(17, 34),
							scaledSize: new google.maps.Size(25, 25)
						};

						// Create a marker for each place.
						markers.push(new google.maps.Marker({
							map: map,
							icon: icon,
							title: place.name,
							position: place.geometry.location
						}));

						if (place.geometry.viewport) {
							// Only geocodes have viewport.
							bounds.union(place.geometry.viewport);
						} else {
							bounds.extend(place.geometry.location);
						}
					});
					map.fitBounds(bounds);
				});
				
				google.maps.event.addListener(map, 'idle', function () {
					matrixPortalPoints.forEach(function (matrixLocatie) {
						if ((matrixLocatie.marker.position.lat() > map.getBounds().f.b && matrixLocatie.marker.position.lat() < map.getBounds().f.f)
						&& (matrixLocatie.marker.position.lng() > map.getBounds().b.b && matrixLocatie.marker.position.lng() < map.getBounds().b.f)
						&& map.zoom > 13) {
							if (!matrixLocatie.isVisible) {
								matrixLocatie.marker.setMap(map);
								matrixLocatie.infoWindow.open(map, matrixLocatie.marker);
								matrixLocatie.updateLiveMatrixImage(matrixLocatie.infoWindow.getContent());
								matrixLocatie.isVisible = true;
							}
						}
						else {
							if (matrixLocatie.isVisible) {
								matrixLocatie.infoWindow.close();
								matrixLocatie.marker.setMap(null)
								matrixLocatie.isVisible = false;
							}
						}
					});
					
					updatePermaLinkAnchorHref();
				});
				
				var trafficLayer = new google.maps.TrafficLayer();
				trafficLayer.setMap(map);
				
				loadMatrixen();
			}

			function loadMatrixen () {
				loadStaticMatrixen();
				setInterval(loadLiveMatrixInfo, 1001*60*2);
			}
			
			function loadStaticMatrixen () {
				loadJson('static/matrixPortaalLocaties.json?1509741368', function(response) {
					var matrixPortalLocations = JSON.parse(response);
					
					matrixPortalLocations.forEach (function (matrixPortalLocation) {
						var coordinate = matrixPortalLocation.coordinate.lat + '_' + matrixPortalLocation.coordinate.lon;
						if (!matrixPortalPoints.has(coordinate)) {						
							var matrixPortalPoint = {};
							matrixPortalPoint.marker = new google.maps.Marker({
								position: new google.maps.LatLng(matrixPortalLocation.coordinate.lat, matrixPortalLocation.coordinate.lon),
								title: matrixPortalLocation.coordinate.lat + ', ' + matrixPortalLocation.coordinate.lon
							});
							matrixPortalPoint.isVisible = false;
							matrixPortalPoint.infoWindow = new google.maps.InfoWindow({
								content: getInfoWindowContent(matrixPortalLocation.matrixPortal.roadWays),
								disableAutoPan: true
							});
							matrixPortalPoint.marker.addListener('click', function() {
								matrixPortalPoint.infoWindow.open(map, matrixPortalPoint.marker);
							});
							matrixPortalPoint.updateLiveMatrixImage = updateLiveMatrixImage;
							
							matrixPortalPoints.set(coordinate, matrixPortalPoint);
						}
					});
					loadLiveMatrixInfo(true);
				});
			}
			
			function loadLiveMatrixInfo (firstLoad) {
				loadJson('live/matrixBorden.json?' + Math.floor(Date.now() / 1000), function(response) {
					var liveMatrixBordenJson = JSON.parse(response);
					
					liveMatrixBordenJson.forEach (function (liveMatrixBord) {
						liveMatrixBorden[liveMatrixBord.uuid] = liveMatrixBord.shownSign;
					});
					
					matrixPortalPoints.forEach(function (matrixLocatie) {
						if (matrixLocatie.isVisible) {
							matrixLocatie.updateLiveMatrixImage(matrixLocatie.infoWindow.getContent());
						}
					});
					
					if (firstLoad === true) {
						google.maps.event.trigger(map, 'idle');
					}
				});
			}
			
			function updateLiveMatrixImage(infoWindowContent) {
				var html = document.createElement('div');
				html.innerHTML = infoWindowContent;
				var imgList = Array.from(html.getElementsByTagName('img'));
				imgList.forEach(function (imgTag) {
					var element = document.getElementById(imgTag.id);
					if (element.title != 'DRIP')
						element.src = 'MSI-afbeeldingen/' + liveMatrixBorden[imgTag.id] + '.png';
					else
						element.src = liveMatrixBorden[imgTag.id];
				});
			}
			
			function updatePermaLinkAnchorHref () {
				var plAnchor = document.getElementById('permaLinkAnchorHref');
				plAnchor.href = '?lat=' + map.center.lat().toFixed(5) + '&lon=' + map.center.lng().toFixed(5) + '&zoom=' + map.zoom;
			}
			
			function getParamByName(name) {
				name = name.replace(/[\[\]]/g, '\\$&');
				
				var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');
				
				var results = regex.exec(window.location.href);
				if (!results || !results[2])  {
					return null;
				}
				
				return decodeURIComponent(results[2].replace(/\+/g, ' '));
			}
		</script>
		<script async defer src="https://maps.googleapis.com/maps/api/js?v=3.exp&key=AIzaSyA3sSJDNgn0zezcAxPqWcrW5wyT67QUkyg&callback=initMap&region=NL&libraries=places"></script>
	</body>
</html>
