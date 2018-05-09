function loadJson(url, callback) {
	var request = new XMLHttpRequest();
	request.overrideMimeType("application/json");
	request.open('GET', url, true);
	request.onload = function () {
		if (this.status >= 200 && this.status < 400) {
			const json = JSON.parse(this.response);
			callback(json);
		}
	};
	request.send();  
}

function post(url, data, callback) {
	var request = new XMLHttpRequest();
	request.open('POST', url, true);
	request.setRequestHeader('Content-Type', 'application/json');
    request.onreadystatechange = function () {
        if (callback && this.readyState == XMLHttpRequest.DONE)
            callback(this.status);
    }
	request.send(JSON.stringify(data));
}

function getInfoWindowContent(country, isLaneSpecific, roadWays) {
	var content = '';

	roadWays.forEach (function (roadWay) {
        if(isLaneSpecific)
        {
            content = content + '<a href="#" onclick="addRoadWayHmLocationForSubscription(\'' + roadWay.HmLocation + '\')">' + roadWay.HmLocation + '</a>';
            content = content + ' (<a href="#" onclick="deleteRoadWayHmLocationForSubscription(\'' + roadWay.HmLocation + '\')">X</a>)<br />';
        }
        else
        {
            content = content + roadWay.HmLocation + '<br />';
        }
		
        roadWay.VariableMessageSigns.forEach(function (vmsLane) {
			var shownSign = 'images/' + country + '/blank.png';
			var laneNumber = 'Rijstrook ' + vmsLane.Number;
            if (!isLaneSpecific) {
				shownSign = '';
                laneNumber = 'DRIP';
			}

            content = content + '<img src="' + shownSign + '" title="' + laneNumber + '" id="' + vmsLane.Id + '" data-islanespecific="' + (isLaneSpecific ? 'true' : 'false') + '" data-country="' + country + '"/>&nbsp;';
		});
		content = content + '<br />';
	});
	
	return content;
}

var map;
var points = new Map();
var liveVmsList = [];
const ShownTypes = {
    BOTH: 0,
    MATRIX: 1,
    DRIP: 2,
};
function initMap() {
	var mapValues = {
		lat: parseFloat(getParamByName('lat') || 52.3393958),
		lon: parseFloat(getParamByName('lon') || 5.3028748),
		zoom: parseFloat(getParamByName('zoom') || 9)
	};
	
	map = new google.maps.Map(document.getElementById('map'), {
		zoom: mapValues.zoom,
		center: new google.maps.LatLng(mapValues.lat, mapValues.lon),
		mapTypeId: 'hybrid',
		rotateControl: false
	});
	map.setTilt(0);

	var searchBoxInput = document.getElementById('searchBox');
	var searchBox = new google.maps.places.SearchBox(searchBoxInput);
	map.controls[google.maps.ControlPosition.TOP_LEFT].push(searchBoxInput);
	
	var typeShownDiv = document.getElementById('typeShown');
	map.controls[google.maps.ControlPosition.TOP_LEFT].push(typeShownDiv);
	
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
	
	loadMatrixen();

    var trafficLayer = new google.maps.TrafficLayer();
	trafficLayer.setMap(map);
	
	google.maps.event.addListener(map, 'idle', function () {
		const typeShownSelect = document.getElementById('typeShown');
		const tssValue = typeShownSelect.options[typeShownSelect.selectedIndex].value;
        points.forEach(function (point) {
            if ((point.marker.position.lat() > map.getBounds().f.b && point.marker.position.lat() < map.getBounds().f.f)
                && (point.marker.position.lng() > map.getBounds().b.b && point.marker.position.lng() < map.getBounds().b.f)
                && map.zoom > 13//) {
			    && pointShouldBeVisible(tssValue, point.isLaneSpecific)) {
                if (!point.isVisible) {
                    point.marker.setMap(map);
                    point.infoWindow.open(map, point.marker);
                    point.updateLiveMatrixImage(point.infoWindow.getContent());
                    point.isVisible = true;
				}
			}
			else {
                if (point.isVisible) {
                    point.infoWindow.close();
                    point.marker.setMap(null)
                    point.isVisible = false;
				}
			}
		});
		
		updatePermaLinkAnchorHref();
	});
}

function pointShouldBeVisible(dropdownValue, pointType) {
    if (dropdownValue == ShownTypes.BOTH)
        return true;

    if (dropdownValue == ShownTypes.MATRIX && pointType)
        return true;

    if (dropdownValue == ShownTypes.DRIP && !pointType)
        return true;

    return false;
}

function loadMatrixen () {
	loadStaticMatrixen();
	setInterval(loadLiveMatrixInfo, 1001*60*2);
}

function loadStaticMatrixen () {
	loadJson('static/locations.json?1525899901', function(locations) {
        locations.forEach (function (location) {
            var coordinate = location.Coordinates.X + '_' + location.Coordinates.Y;
			if (!points.has(coordinate)) {						
				var point = {};
                point.marker = new google.maps.Marker({
                    position: new google.maps.LatLng(location.Coordinates.X, location.Coordinates.Y),
                    title: location.Coordinates.X + ', ' + location.Coordinates.Y
				});

                point.isVisible = false;
                point.infoWindow = new google.maps.InfoWindow({
                    content: getInfoWindowContent(location.Country, location.IsLaneSpecific, location.RoadWays),
					disableAutoPan: true
                });
                point.isLaneSpecific = location.IsLaneSpecific;
                point.marker.addListener('click', function() {
                    point.infoWindow.open(map, point.marker);
				});
                point.updateLiveMatrixImage = updateLiveMatrixImage;
				
                points.set(coordinate, point);
			}
		});
		loadLiveMatrixInfo(true);
	});
}

function loadLiveMatrixInfo (firstLoad) {
    loadJson('live/liveData.json?' + Math.floor(Date.now() / 1000), function(liveData) {
        liveData.forEach (function (liveVms) {
			liveVmsList[liveVms.Id] = liveVms.Sign;
		});
		
        points.forEach(function (point) {
            if (point.isVisible) {
                point.updateLiveMatrixImage(point.infoWindow.getContent());
			}
		});
		
		if (firstLoad) {
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
		var imageSource = liveVmsList[imgTag.id];
		if (element.getAttribute('data-islanespecific') == 'true')
			element.setAttribute('src', 'images/' + element.getAttribute('data-country') + '/' + imageSource + '.png');
		else
			element.setAttribute('src', !liveVmsList[imgTag.id] ? '' : liveVmsList[imgTag.id]);
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

document.addEventListener('DOMContentLoaded',function() {
    document.querySelector('select[name="typeShown"]').onchange = onTypeShownChanged;
}, false);


function onTypeShownChanged (event) {
	google.maps.event.trigger(map, 'idle');
}

function urlB64ToUint8Array(base64String) {
	const padding = '='.repeat((4 - base64String.length % 4) % 4);
	const base64 = (base64String + padding)
				   .replace(/\-/g, '+')
				   .replace(/_/g, '/');

	const rawData = window.atob(base64);
	const outputArray = new Uint8Array(rawData.length);

	for (let i = 0; i < rawData.length; ++i) {
		outputArray[i] = rawData.charCodeAt(i);
	}
	
	return outputArray;
}

if ('serviceWorker' in navigator && 'PushManager' in window) {
	navigator.serviceWorker.register('serviceWorker.js').then(function(swReg) {
		swRegistration = swReg;
	}).catch(function(error) {
		// TODO: Handle errors
	});
} else {
    // TODO: Handle errors
	//alert('Helaas, je browser ondersteunt geen Web Push Notifications. Probeer het eens in Chrome? Neem anders contact op met de ontwikkelaar!');
}

const applicationServerPublicKey = 'BOPhcWmoRn0wpBvBPTzDrFzIyH4IZ62olqNnl1ZVGMCDh8UEZuydKTkrlh89ZaHVCzZPnjlKdEZH4Q88hC1Wrps';

function addRoadWayHmLocationForSubscription(hmLocation) {
	const applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
	swRegistration.pushManager.subscribe({
		userVisibleOnly: true,
		applicationServerKey: applicationServerKey
	}).then(function(subscription) {
		subscriptionAddRoadWayHmLocation(subscription, hmLocation);
	}).catch(function(error) {
		// TODO: Handle errors
	});
}

function deleteRoadWayHmLocationForSubscription(hmLocation) {
	swRegistration.pushManager.getSubscription()
    .then(function(subscription) {
		if (!(subscription === null))
            subscriptionDeleteRoadWayHmLocation(subscription, hmLocation);
	}).catch(function(error) {
		// TODO: Handle errors
	});
}

function subscriptionAddRoadWayHmLocation(subscription, hmLocation) {
	const data = {
		subscription: btoa(JSON.stringify(subscription)),
		hmLocation: hmLocation
	};
    
    let addText = 'Je zou binnen 2 minuten een eerste notificatie moeten ontvangen van ' + hmLocation + '. Bij wijziging van een matrixbord krijg je weer een nieuwe notificatie!';
	post('api/Subscription/addRoadWayHmLocation.php', data, function (statusCode) {
        if (statusCode != 200)
            addText = 'Helaas, er is iets mis gegaan met notificaties inschakelen voor ' + hmLocation + '. Neem eventueel contact op met de ontwikkelaar!';
        
        alert (addText);
    });
}

function subscriptionDeleteRoadWayHmLocation(subscription, hmLocation) {
	const data = {
		subscription: btoa(JSON.stringify(subscription)),
		hmLocation: hmLocation
	};
    let deleteText = 'We hebben je notificaties uitgeschakeld voor ' + hmLocation + '.';
    
	post('api/Subscription/deleteRoadWayHmLocation.php', data, function (statusCode) {
        if (statusCode != 200)
        {
            if (statusCode == 204)
            {
                subscription.unsubscribe();
                //deleteText = deleteText + ' Bij een nieuwe locatie zul je weer opnieuw de notificaties moeten toestaan.';
            }
            else
            {
                deleteText = 'Helaas, er is iets mis gegaan met notificaties uitschakelen voor ' + hmLocation + '. Neem eventueel contact op met de ontwikkelaar!';
            }
        }
        
        alert(deleteText);
    });
}
