const config = { Url: '__ApiUrl__' };
let inHistoryModus = false;
let historyIsSearchingAndLoadingTimeout = null;

document.getElementById('notificationList').hidden = true;
document.getElementById('historyDtPicker').hidden = true;
if ('serviceWorker' in navigator && 'PushManager' in window) {
    navigator.serviceWorker.register('serviceWorker.js').then(function (swReg) {
        swRegistration = swReg;
    }).catch(function (error) {
        // TODO: Handle errors
    });
} else {
    // TODO: Handle errors
    //alert('Helaas, je browser ondersteunt geen Web Push Notifications. Probeer het eens in Chrome? Neem anders contact op met de ontwikkelaar!');
}

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

function load2Json(url) {
    return new Promise(function (resolve, reject) {
        var xmlHttpRequest = new XMLHttpRequest();
        xmlHttpRequest.open('GET', url);

        xmlHttpRequest.onload = function () {
            if (xmlHttpRequest.status == 200) {
                resolve(xmlHttpRequest.response);
            }
            else {
                reject(Error(xmlHttpRequest.status));
            }
        };

        xmlHttpRequest.onerror = function () {
            reject(Error('Network Error'));
        };

        xmlHttpRequest.send();
    });
}

function loadMatrixJson(fileName) {
    return new Promise(function (resolve) {
        const originalFilename = fileName;
        if (fileName !== 'liveData' && inHistoryModus) {
            fileName = 'history/' + fileName.substring(0, 16).replace(':', '_');
        }

        load2Json('live/' + fileName + '.json?' + Math.floor(Date.now() / 1000)).then(function (result) {
            resolve(result);
        }).catch(function (error) {
            if (inHistoryModus && error.message === "404") {
                const newFileName = new Date(new Date(originalFilename).getTime() - 60000).toISOString();
                resolve(loadMatrixJson(newFileName));
            }
        });
    });
}

function sendRequest(type, url, data, callback) {
    var request = new XMLHttpRequest();
    request.open(type, url, true);
    request.setRequestHeader('Content-Type', 'application/json');
    request.onreadystatechange = function () {
        if (callback && this.readyState == XMLHttpRequest.DONE)
            callback(this.status);
    };
    request.send(JSON.stringify(data));
}

function getInfoWindowContent(country, isLaneSpecific, roadWays) {
    let content = '';

    roadWays.forEach(function (roadWay) {
        content = content + '<a href="#" onclick="addRoadWayHmLocationForSubscription(\'' + roadWay.HmLocation + '\')">' + roadWay.HmLocation + '</a>';
        content = content + ' (<a href="#" onclick="deleteRoadWayHmLocationForSubscription(\'' + roadWay.HmLocation + '\')">X</a>)<br />';

        roadWay.VariableMessageSigns.forEach(function (vmsLane) {
            let shownSign = 'images/' + country + '/blank.png';
            let laneNumber = 'Rijstrook ' + vmsLane.Number;
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

function fillNotificationList() {
    swRegistration.pushManager.getSubscription().then(function (subscription) {
        if (subscription !== null) {
            document.getElementById('notificationList').hidden = false;
            loadJson(config.Url + '/api/UserSubscription?pushSubscriptionEndpoint=' + encodeURI(subscription.endpoint), handleNotificationList);
        }
    });
}

function handleNotificationList(result) {
    result.NotificationList.forEach(addOptionToNotficationList);
}

function addOptionToNotficationList(notification) {
    const option = document.createElement('option');
    option.value = notification;
    option.innerHTML = notification;

    notificationList.add(option);
}

function deleteOptionFromNotficationList(notification) {
    const notificationList = document.getElementById('notificationList');
    const optionsAmount = notificationList.options.length;
    for (let i = 0; i < optionsAmount; i++) {
        if (notificationList.options[i].value == notification)
            notificationList.remove(i);
    }
}

let map;
let firstLoad = true;
const points = new Map();
const roadWayPoints = new Map();
const liveVmsList = [];
const ShownType = {
    Both: 0,
    Matrix: 1,
    Drip: 2,
};

function initMap() {
    const mapValues = {
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

    const searchBoxInput = document.getElementById('searchBox');
    const searchBox = new google.maps.places.SearchBox(searchBoxInput);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(searchBoxInput);

    const typeShown = document.getElementById('typeShown');
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(typeShown);

    const notificationList = document.getElementById('notificationList');
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(notificationList);

    const historyDtPicker = document.getElementById('historyDtPicker');
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(historyDtPicker);

    const historyButton = document.getElementById('historyButton');
    map.controls[google.maps.ControlPosition.RIGHT_TOP].push(historyButton);

    //const signsCopyGertDiv = document.getElementById('signsCopyGert');
    //map.controls[google.maps.ControlPosition.TOP_LEFT].push(signsCopyGertDiv);

    const permaLinkDiv = document.getElementById('permaLink');
    map.controls[google.maps.ControlPosition.RIGHT_BOTTOM].push(permaLinkDiv);

    // Bias the SearchBox results towards current map's viewport.
    map.addListener('bounds_changed', function () {
        searchBox.setBounds(map.getBounds());
    });

    let markers = [];
    // Listen for the event fired when the user selects a prediction and retrieve
    // more details for that place.
    searchBox.addListener('places_changed', function () {
        const places = searchBox.getPlaces();

        if (places.length == 0) {
            return;
        }

        // Clear out the old markers.
        markers.forEach(function (marker) {
            marker.setMap(null);
        });
        markers = [];

        // For each place, get the icon, name and location.
        const bounds = new google.maps.LatLngBounds();
        places.forEach(function (place) {
            if (!place.geometry) {
                console.log("Returned place contains no geometry");
                return;
            }
            const icon = {
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

    const trafficLayer = new google.maps.TrafficLayer();
    trafficLayer.setMap(map);

    google.maps.event.addListener(map, 'idle', function () {
        if (firstLoad) {
            loadMatrixInfo();
            firstLoad = false;
        }

        placePointsOnMap().then(function (visiblePoints) {
            setTimeout(function () {
                visiblePoints.forEach(function (visiblePoint) {
                    updateLiveMatrixImage(visiblePoint.infoWindow.getContent());
                });
            }, 1000);
        });

        updatePermaLinkAnchorHref();
    });

    setTimeout(fillNotificationList, 1000);
}

function toggleHistoryModus() {
    if (!inHistoryModus) {
        inHistoryModus = true;
        document.querySelector('#typeShown [value="' + ShownType.Matrix + '"]').selected = true;
        document.getElementById('notificationList').hidden = true;
    }
    else {
        fillNotificationList();
        inHistoryModus = false;
        document.querySelector('#typeShown [value="' + ShownType.Both + '"]').selected = true;
    }

    document.getElementById('typeShown').disabled = inHistoryModus;
    document.getElementById('historyDtPicker').hidden = !inHistoryModus;
    triggerGoogleMapsIdleEvent();
}

function placePointsOnMap() {
    return new Promise(function (resolve, reject) {
        const visiblePoints = new Map();
        const typeShownSelect = document.getElementById('typeShown');
        const tssValue = typeShownSelect.options[typeShownSelect.selectedIndex].value;
        points.forEach(function (point, coordinate) {
            if ((point.marker.position.lat() > map.getBounds().getSouthWest().lat() && point.marker.position.lat() < map.getBounds().getNorthEast().lat())
                && (point.marker.position.lng() > map.getBounds().getSouthWest().lng() && point.marker.position.lng() < map.getBounds().getNorthEast().lng())
                && map.zoom > 13
                && pointShouldBeVisible(tssValue, point.isLaneSpecific)) {
                if (!point.isVisible) {
                    point.marker.setMap(map);
                    point.infoWindow.open(map, point.marker);
                    point.isVisible = true;
                }

                if (!visiblePoints.has(coordinate)) {
                    visiblePoints.set(coordinate, point);
                }
            }
            else {
                if (point.isVisible) {
                    point.infoWindow.close();
                    point.marker.setMap(null);
                    point.isVisible = false;
                    if (visiblePoints.has(coordinate)) {
                        visiblePoints.delete(coordinate);
                    }
                }
            }
        });

        resolve(visiblePoints);
    });
}

function pointShouldBeVisible(dropdownValue, pointType) {
    if (dropdownValue == ShownType.Both)
        return true;

    if (dropdownValue == ShownType.Matrix && pointType)
        return true;

    if (dropdownValue == ShownType.Drip && !pointType)
        return true;

    return false;
}

function loadMatrixInfo() {
    loadStaticMatrixen();
    loadLiveMatrixInfo();
    setInterval(loadLiveMatrixInfo, 1001 * 60);
}

function loadStaticMatrixen() {
    load2Json('static/locations.json?1567897244').then(function (locationsJson) {
        return JSON.parse(locationsJson);
    }).then(function (locations) {
        locations.forEach(function (location) {
            const coordinate = location.Coordinates.X + '_' + location.Coordinates.Y;
            if (!points.has(coordinate)) {
                const point = {};
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
                point.marker.addListener('click', function () {
                    point.infoWindow.open(map, point.marker);
                });

                points.set(coordinate, point);

                location.RoadWays.forEach(function (roadWay) {
                    roadWayPoints.set(roadWay.HmLocation, coordinate);
                });
            }
        });
    }).then(function () {
        triggerGoogleMapsIdleEvent();
    });
}

function loadLiveMatrixInfo(fileName = 'liveData') {
    if (fileName === 'liveData' && inHistoryModus)
        return;

    loadMatrixJson(fileName).then(function (liveDataJson) {
        return JSON.parse(liveDataJson);
    }).then(function (liveData) {
        liveData.forEach(function (liveVms) {
            liveVmsList[liveVms.Id] = liveVms.Sign;
        });
    }).then(function () {
        triggerGoogleMapsIdleEvent();
    });
}

function isInteger(value) {
    if (/^(\-|\+)?([0-9]+|Infinity)$/.test(value))
        return true;

    return false;
}

function updateLiveMatrixImage(infoWindowContent) {
    const html = document.createElement('div');
    html.innerHTML = infoWindowContent;
    const imgList = Array.from(html.getElementsByTagName('img'));
    imgList.forEach(function (imgTag) {
        const element = document.getElementById(imgTag.id);
        if (element.getAttribute('data-islanespecific') == 'true') {
            const shownSign = !liveVmsList[imgTag.id] ? 'unknown' : liveVmsList[imgTag.id];
            element.setAttribute('src', 'images/' + element.getAttribute('data-country') + '/' + shownSign + '.png');
        }
        else {
            updateNonLaneSpecificVms(element, imgTag);
        }
    });
}

function updateNonLaneSpecificVms(element, imgTag) {
    if (isInteger(liveVmsList[imgTag.id]))
        element.setAttribute('src', !liveVmsList[imgTag.id] ? '' : 'live/images/VMS/' + imgTag.id);
    else {
        let divElement = null;
        if (element.tagName.toLowerCase() == 'img') {
            divElement = document.createElement('div');
            divElement.setAttribute('title', element.getAttribute('title'));
            divElement.setAttribute('id', element.getAttribute('id'));
            divElement.setAttribute('data-islanespecific', element.getAttribute('data-islanespecific'));
            divElement.setAttribute('data-country', element.getAttribute('data-country'));
            divElement.setAttribute('class', 'textVms');
            element.parentNode.replaceChild(divElement, element);
        }
        else {
            divElement = element;
        }

        if (liveVmsList[imgTag.id])
            divElement.innerHTML = liveVmsList[imgTag.id];
    }
}

function updatePermaLinkAnchorHref() {
    const plAnchor = document.getElementById('permaLinkAnchorHref');
    plAnchor.href = '?lat=' + map.center.lat().toFixed(5) + '&lon=' + map.center.lng().toFixed(5) + '&zoom=' + map.zoom;
}

function getParamByName(name) {
    name = name.replace(/[\[\]]/g, '\\$&');

    const regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');

    const results = regex.exec(window.location.href);
    if (!results || !results[2]) {
        return null;
    }

    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

window.addEventListener('load', function () {
    document.querySelector('select[name="typeShown"]').onchange = onTypeShownChanged;
    document.querySelector('select[name="notificationList"]').onchange = onNotificationListChanged;
    document.getElementById('historyDtPicker').onchange = onHistoryDtPickerChanged;
}, false);

function onTypeShownChanged(event) {
    triggerGoogleMapsIdleEvent();
}

function onNotificationListChanged(event) {
    const notificationListSelect = document.getElementById('notificationList');
    const nlValue = notificationListSelect.options[notificationListSelect.selectedIndex].value.toString();
    if (nlValue === 'Notificatielijst')
        return;

    const coordinates = roadWayPoints.get(nlValue).split('_');
    map.setCenter({
        lat: parseFloat(coordinates[0]),
        lng: parseFloat(coordinates[1])
    });
    map.setZoom(17);
}

function onHistoryDtPickerChanged(event) {
    clearTimeout(historyIsSearchingAndLoadingTimeout);

    historyIsSearchingAndLoadingTimeout = setTimeout(function () {
        const localDt = new Date(event.target.value);
        const fileName = new Date(localDt.getTime()).toISOString();
        loadLiveMatrixInfo(fileName);
    }, 750);
}

function triggerGoogleMapsIdleEvent() {
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

const applicationServerKey = urlB64ToUint8Array('BOPhcWmoRn0wpBvBPTzDrFzIyH4IZ62olqNnl1ZVGMCDh8UEZuydKTkrlh89ZaHVCzZPnjlKdEZH4Q88hC1Wrps');

function addRoadWayHmLocationForSubscription(hmLocation) {
    swRegistration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey
    }).then(function (subscription) {
        subscriptionAddRoadWayHmLocation(subscription, hmLocation);
    }).catch(function (error) {
        console.error('addRoadWayHmLocationForSubscription: ' + error);
    });
}

function deleteRoadWayHmLocationForSubscription(hmLocation) {
    swRegistration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey
    }).then(function (subscription) {
        subscriptionDeleteRoadWayHmLocation(subscription, hmLocation);
    }).catch(function (error) {
        console.error('deleteRoadWayHmLocationForSubscription: ' + error);
    });
}

function subscriptionAddRoadWayHmLocation(subscription, hmLocation) {
    const data = {
        pushSubscription: subscription,
        hmLocation: hmLocation
    };

    sendRequest('POST', config.Url + '/api/UserSubscription', data, function (statusCode) {
        let addText = '';
        if (statusCode == 201) {
            addText = 'Genoteerd, je zou binnen 2 minuten een eerste notificatie moeten ontvangen van ' + hmLocation + '. Bij wijziging krijg je weer een nieuwe notificatie!';
            addOptionToNotficationList(hmLocation);
            document.getElementById('notificationList').hidden = false;
        }
        else if (statusCode == 409) {
            addText = 'Interessant, je hebt al notificaties ingeschakeld voor ' + hmLocation + '. Klopt dit niet? Neem eventueel contact op met de ontwikkelaar!';
        }
        else {
            addText = 'Helaas, er is iets mis gegaan met notificaties inschakelen voor ' + hmLocation + '. Neem eventueel contact op met de ontwikkelaar!';
        }

        alert(addText);
    });
}

function subscriptionDeleteRoadWayHmLocation(subscription, hmLocation) {
    const data = {
        pushSubscription: subscription,
        hmLocation: hmLocation
    };

    sendRequest('DELETE', config.Url + '/api/UserSubscription', data, function (statusCode) {
        let deleteText = '';
        if (statusCode == 200) {
            deleteText = 'Begrijpelijk, we hebben je notificaties uitgeschakeld voor ' + hmLocation + '.';
            deleteOptionFromNotficationList(hmLocation);
        }
        else if (statusCode == 204) {
            subscription.unsubscribe().then(function (success) {
                alert('Jammer, nu we je notificaties hebben uitgeschakeld voor ' + hmLocation + ' blijft er niks meer over. We hebben je uit ons bestand gehaald. Het is uiteraard nog steeds mogelijk om weer nieuwe notificaties in te schakelen!');
                deleteOptionFromNotficationList(hmLocation);
                document.getElementById('notificationList').hidden = true;
            }).catch(function (e) {
                console.error('subscriptionDeleteRoadWayHmLocation - statusCode == 204: ' + e);
            });
        }
        else if (statusCode == 409) {
            deleteText = 'Interessant, mogelijk kreeg je al geen notificaties meer voor ' + hmLocation + ' en/of andere locaties. Klopt dit niet? Neem eventueel contact op met de ontwikkelaar!';
        }
        else if (statusCode == 410) {
            deleteText = 'Je bent niet meer bij ons bekend in het bestand met betrekking tot enige notificaties. Het is uiteraard mogelijk om weer nieuwe notificaties in te schakelen!';
        }
        else {
            deleteText = 'Helaas, er is iets mis gegaan met notificaties uitschakelen voor ' + hmLocation + '. Neem eventueel contact op met de ontwikkelaar!';
        }

        if (deleteText !== '')
            alert(deleteText);
    });
}
