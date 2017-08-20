<!DOCTYPE html>	
<html>
	<head>
		<meta charset="UTF-8">
		<title>Matrix</title>
		<meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
		<style>
			#map {
				height: 100%;
			}

			html, body {
				height: 100%;
				margin: 0;
				padding: 0;
			}
			
			#description {
				font-family: Roboto;
				font-size: 15px;
				font-weight: 300;
			}

			#infowindow-content .title {
				font-weight: bold;
			}

			#infowindow-content {
				display: none;
			}

			#map #infowindow-content {
				display: inline;
			}

			.pac-card {
				margin: 10px 10px 0 0;
				border-radius: 2px 0 0 2px;
				box-sizing: border-box;
				-moz-box-sizing: border-box;
				outline: none;
				box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
				background-color: #fff;
				font-family: Roboto;
			}

			#pac-container {
				padding-bottom: 12px;
				margin-right: 12px;
			}

			.pac-controls {
				display: inline-block;
				padding: 5px 11px;
			}

			.pac-controls label {
				font-family: Roboto;
				font-size: 13px;
				font-weight: 300;
			}

			#searchBox {
				background-color: #fff;
				font-family: Roboto;
				font-size: 15px;
				font-weight: 300;
				margin-left: 12px;
				padding: 0 11px 0 13px;
				text-overflow: ellipsis;
				width: 200px;
				top: 9.4px !important;
				height: 26.2px;
				left: 92px !important;
			}

			#searchBox:focus {
				border-color: #4d90fe;
			}

			#title {
				color: #fff;
				background-color: #4d90fe;
				font-size: 25px;
				font-weight: 500;
				padding: 6px 12px;
			}
			
			#target {
				width: 345px;
			}
			
			#signsCopyGert {
				font-family: Roboto, Arial, sans-serif;
				color: rgb(68, 68, 68);
				font-size: 10px;
				background: #fff;
				opacity: 0.7;
				padding: 1px;
				left: 10px !important;
				top: 39px !important;
				width: 93px;
				text-align: center;
			}
		</style>
	</head>
	<body>
		<input id="searchBox" class="controls" type="text" placeholder="Search" />
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
						var shownSign = 'onbekend';
						if (lane.shownSign != null)
							shownSign = lane.shownSign;
						
						iwContent = iwContent + '<img src="MSI-afbeeldingen/' + shownSign + '.png" title="Rijstrook ' + lane.number + '" id="' + lane.uuid + '" />&nbsp;';
					});
					iwContent = iwContent + '<br />';
				});
				
				return iwContent;
			}

			var map;
			var matrixPortalPoints = new Map();
			var liveMatrixBorden = [];
			function initMap() {
				map = new google.maps.Map(document.getElementById('map'), {
					zoom: 9,
					center: new google.maps.LatLng(52.3393958,5.3028748),
					mapTypeId: 'hybrid'
				});

				var searchBoxInput = document.getElementById('searchBox');
				var searchBox = new google.maps.places.SearchBox(searchBoxInput);
				map.controls[google.maps.ControlPosition.TOP_LEFT].push(searchBoxInput);
				
				var signsCopyGertDiv = document.getElementById('signsCopyGert');
				map.controls[google.maps.ControlPosition.TOP_LEFT].push(signsCopyGertDiv);

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
				
				google.maps.event.addListener(map, 'idle', function() {
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
				});
				
				var trafficLayer = new google.maps.TrafficLayer();
				trafficLayer.setMap(map);
				
				loadMatrixen();
			}

			function loadMatrixen () {
				loadStaticMatrixen();
				loadLiveMatrixInfo();
				setInterval(loadLiveMatrixInfo, 1001*60*2);
			}
			
			function loadStaticMatrixen () {
				loadJson('static/matrixPortaalLocaties.json', function(response) {
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
				});
			}
			
			function loadLiveMatrixInfo () {
				loadJson('live/matrixBorden.json', function(response) {
					var liveMatrixBordenJson = JSON.parse(response);
					
					liveMatrixBordenJson.forEach (function (liveMatrixBord) {
						liveMatrixBorden[liveMatrixBord.uuid] = liveMatrixBord.shownSign;
					});
					
					matrixPortalPoints.forEach(function (matrixLocatie) {
						if (matrixLocatie.isVisible) {
							matrixLocatie.updateLiveMatrixImage(matrixLocatie.infoWindow.getContent());
						}
					});
				});
			}
			
			function updateLiveMatrixImage(infoWindowContent) {
				var html = document.createElement('div');
				html.innerHTML = infoWindowContent;
				var imgList = Array.from(html.getElementsByTagName('img'));
				imgList.forEach(function (imgTag) {
					//setTimeout (function () { document.getElementById(imgTag.id).src = 'MSI-afbeeldingen/' + liveMatrixBorden[imgTag.id] + '.png'; }, 1);
					document.getElementById(imgTag.id).src = 'MSI-afbeeldingen/' + liveMatrixBorden[imgTag.id] + '.png';
					
				});
			}
		</script>
		<script async defer src="https://maps.googleapis.com/maps/api/js?v=3.exp&key=AIzaSyA3sSJDNgn0zezcAxPqWcrW5wyT67QUkyg&callback=initMap&region=NL&libraries=places"></script>
	</body>
</html>