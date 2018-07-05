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
        event.waitUntil(self.registration.showNotification(notification.title, notification));
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

//const showNotificationPromise = self.registration.getNotifications().then(notifications => {
//    let currentNotification;

//    for (let i = 0; i < notifications.length; i++) {
//        debugger;
//        if (notifications[i].data && notifications[i].data.userName === userName) {
//            currentNotification = notifications[i];
//        }
//    }

//    return currentNotification;
//}).then((currentNotification) => {
//    //let notificationTitle;
//    //const options = {
//    //    icon: userIcon,
//    //}

//    //if (currentNotification) {
//    //    // We have an open notification, let's do something with it.
//    //    const messageCount = currentNotification.data.newMessageCount + 1;

//    //    options.body = `You have ${messageCount} new messages from ${userName}.`;
//    //    options.data = {
//    //        userName: userName,
//    //        newMessageCount: messageCount
//    //    };
//    //    notificationTitle = `New Messages from ${userName}`;

//    //    // Remember to close the old notification.
//    //    currentNotification.close();
//    //} else {
//    //    options.body = `"${userMessage}"`;
//    //    options.data = {
//    //        userName: userName,
//    //        newMessageCount: 1
//    //    };
//    //    notificationTitle = `New Message from ${userName}`;
//    //}

//    return registration.showNotification("test", { message: "bla" });
//});
