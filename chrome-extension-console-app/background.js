/*
On startup, connect to the "co.cortexcloud.thai_id_card_reader" app.
*/
var port = chrome.runtime.connectNative("co.cortexcloud.thai_id_card_reader_console_app");
var tab = null;

/*
Listen for messages from the app.
*/
port.onMessage.addListener((response) => {
  console.log("Received: " + JSON.stringify(response));
 

  onSendToWeb(response);
});


console.log('Background script is started.')

function onDisconnect() {
    port.onDisconnect.addListener(function () {
        if (chrome.runtime.lastError) {
            console.log(chrome.runtime.lastError);
        }
    });
}

function  onSendToWeb(resp)
{
    chrome.tabs.query({active: true, currentWindow: true}, function(tabs) {
        tabs.forEach(function(tab) {
            console.log('Tab ID: ', tab.id);
            // send data through a DOM event
            //window.postMessage({greeting: "hello"});
            //document.dispatchEvent(new CustomEvent('csEvent',{greeting: "hello"}));
            chrome.tabs.sendMessage(tab.id, {greeting: "hello"});
        });
    });

    //chrome.tabs.sendMessage(tab.id, {greeting: "hello"});

    onDisconnect();
}