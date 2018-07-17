self.addEventListener('push', function (event) {
    if (!event.data.json().title) {
        const notification = JSON.parse(event.data.text());
        const title = notification.hmLocation;
        const options = {};
        options.body = notification.lanesShownSign;
        if (notification.dripShownSign)
            options.image = notification.dripShownSign;

        event.waitUntil(self.registration.showNotification(title, options));
    }
    else {
        const notification = event.data.json();
		for (var propertyName in notification) {
			if (!notification[propertyName])
				delete notification[propertyName];
		}			
        event.waitUntil(showNotification(notification.title, notification));
    }
});

self.addEventListener('notificationclick', function (event) {
    const notification = event.notification;
    if (!event.action) { // Was a normal notification click
        var image = notification.image;
        if (!image.endsWith('null'))
            event.waitUntil(clients.openWindow(image));
        return;
    }

    switch (event.action) {
        case 'go-to-location':
            event.waitUntil(clients.openWindow(notification.data.coordinatesUrl));
            break;
        default:
            console.log('Not yet implemented! :(');
            break;
    }
});

function showNotification(title, currentNotification) {
	return self.registration.getNotifications({ tag: currentNotification.tag }).then(notifications => {
		let previousNotification;

		const amount = notifications.length;
		for (let i = 0; i < amount; i++) {
			if (notifications[i].tag && notifications[i].tag === currentNotification.tag) {
				previousNotification = notifications[i];
			}
		}
		
		return previousNotification;
	}).then(previousNotification => {
		if (!(!previousNotification)) {
			const previousBody = previousNotification.body;
			currentNotification.body += '\n' + previousBody;
			currentNotification.renotify = true;
		}
		
		return self.registration.showNotification(title, currentNotification);
	});
}
