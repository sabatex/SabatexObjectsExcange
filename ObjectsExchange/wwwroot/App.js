(async function () {

    window.blazorPushNotifications = {
        requestSubscription: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            const existingSubscription = await worker.pushManager.getSubscription();
            if (!existingSubscription) {
                const newSubscription = await subscribe(worker);
                if (newSubscription) {
                    return {
                        url: newSubscription.endpoint,
                        p256dh: arrayBufferToBase64(newSubscription.getKey('p256dh')),
                        auth: arrayBufferToBase64(newSubscription.getKey('auth'))
                    };
                }
            }
        }
    };

    async function subscribe(worker) {
        try {

            const responce = await fetch("/api/push/public_key");
            if (!responce.ok) {
                console.error('Error get public key')
                return false;
            }

            const applicationServerPublicKey = await responce.json();

            return await worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerPublicKey.cert
            });
        } catch (error) {
            if (error.name === 'NotAllowedError') {
                return null;
            }
            throw error;
        }
    }
    function arrayBufferToBase64(buffer) {
        // https://stackoverflow.com/a/9458996
        var binary = '';
        var bytes = new Uint8Array(buffer);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
})();


//var subscribe = blazorPushNotifications.requestSubscription();
//if (subscribe != null) {
//    await fetch("notifications/subscribe")
//}
let rz_sidebar_toggle = document.querySelector('.rz-sidebar-toggle');
rz_sidebar_toggle.addEventListener("click", function () {
    let sidebar = document.querySelector('.rz-sidebar');
    let isButtonCollapse = sidebar.classList.contains("side-bar-collapse");
    console.log("isButtonCollapse = " + isButtonCollapse);
    let isCollapsed = sidebar.clientWidth == 0;
    console.log("isCollapsed = " + isCollapsed);

    if (isButtonCollapse) {
        sidebar.classList.remove("side-bar-collapse");
        if (sidebar.clientWidth == 0) {
            sidebar.classList.remove("rz-sidebar-responsive");
        }
        console.log("SideBar Show");
    } else {
        if (isCollapsed && sidebar.classList.contains("rz-sidebar-responsive")) {
            sidebar.classList.remove("rz-sidebar-responsive");
        }
        else {
            sidebar.classList.add("side-bar-collapse");
            if (!sidebar.classList.contains("rz-sidebar-responsive")) {
                sidebar.classList.add("rz-sidebar-responsive");
            }
        }
        console.log("SideBar Close");
    }

})


function toggleTheme() {
    let theme = localStorage.getItem("sabatexTheme");
    if (theme == "-dark") {
        theme = "";
    } else {
        theme = "-dark"
    }
    localStorage.setItem("sabatexTheme", theme);
    var link = document.getElementById("sabatex-radzenCSS");
    if (link) {
        const href = "_content/Radzen.Blazor/css/material" + theme + "-base.css";
        link.href = href;
    }

}