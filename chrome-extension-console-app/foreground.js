console.log('foreground script is started.')

var port = chrome.runtime.connect({name: "cortex"});


function onNotification(msg){
    port.postMessage({ action : 'notification', value : msg });
}

function onConnect(){
    port.postMessage({ action : 'connect' });
}


function onRetry(){
    port.postMessage({ action : 'retry' });
}

function onCancel(){
    port.postMessage({ action : 'cancel' });
}

// //listener message from background
// port.onMessage.addListener(function(message) {
//     console.log(message.resp)
// });

chrome.runtime.onMessage.addListener(
    function(request, sender, sendResponse) {
        
        //for test
        //document.getElementById("result").value = JSON.stringify(request.data); 
  
        const event = new CustomEvent('message', {detail : request.data});
        window.dispatchEvent(event);

        // var btnRetry = document.getElementById('btnRetry');
        // btnRetry.addEventListener('click', () => {
        //     onConnect();
        // });

    }
);

// var btnConnect = document.getElementById('btnConnect');
// btnConnect.addEventListener('click', () => {
//     onConnect();
// });
