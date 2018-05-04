self.addEventListener('push', function(event) {
    const notification = JSON.parse(event.data.text());
    const title = notification.hmLocation;
    const options = {};
    options.body = notification.lanesShownSign;
    if (notification.dripShownSign)
        options.image = notification.dripShownSign;

    event.waitUntil(self.registration.showNotification(title, options));
});
/*
self.addEventListener('notificationclick', function(event) {
    const notification = JSON.parse(event.data.text());
    const urlToOpen = new URL(notification.url, self.location.origin).href;

    const promiseChain = clients.matchAll({
        type: 'window',
        includeUncontrolled: true
    })
    .then((windowClients) => {
        let matchingClient = null;

        for (let i = 0; i < windowClients.length; i++) {
            const windowClient = windowClients[i];
            if (windowClient.url === urlToOpen) {
                matchingClient = windowClient;
                break;
            }
        }

        if (matchingClient) {
            return matchingClient.focus();
        } else {
            return clients.openWindow(urlToOpen);
        }
    });

    event.waitUntil(promiseChain);
});
*/
