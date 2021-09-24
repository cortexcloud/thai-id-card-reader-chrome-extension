
console.log('Background script is started.')

/* On startup, connect to the "co.cortexcloud.thai_id_card_reader" app. */
var host = 'co.cortexcloud.thai_id_card_reader_console_app';
var appPort = null;

onConnectToNativeApp();

chrome.tabs.onActivated.addListener(tab => {
    chrome.tabs.get(tab.tabId, current_tab_info => {
        //check target current web to send data
        if(/^TEST/.test(current_tab_info.title))
        {
            //execute foreground script
            chrome.tabs.executeScript(null,{file: './foreground.js'},() => {
   
            });
        }
    });
});

//listner from web cortex
chrome.runtime.onConnect.addListener(function(port) {
    //allow from forground port name thaiidcard
    if(port.name === 'cortex')
    {
        port.onMessage.addListener(function(resp) {
            if(resp.action === 'notification')
            {
               console.log('notification : ' + resp.value);
            }
            if(resp.action === 'connect')
            {
                onDisconnect();
                onConnectToNativeApp();
            }
            if(resp.action === 'cancel')
            {
                onDisconnect();
            }
            // if (msg.joke == "Knock knock")
            //     port.postMessage({question: "Who's there?"});
            // else if (msg.answer == "Madame")
            //     port.postMessage({question: "Madame who?"});
            // else if (msg.answer == "Madame... Bovary")
            //     port.postMessage({question: "I don't get it."});
        });
    }

    
  });

function onConnectToNativeApp() {
   
    console.log('connect to native app');

    appPort = chrome.runtime.connectNative("co.cortexcloud.thai_id_card_reader_console_app");

    /* Listen for messages from the app.*/
    appPort.onMessage.addListener((resp) => {
        console.log("Received: " + JSON.stringify(resp));

        chrome.tabs.query({active: true, currentWindow: true}, function(tabs) {
            chrome.tabs.sendMessage(tabs[0].id,{ data : resp });

            // chrome.tabs.sendMessage(tabs[0].id,{ message : resp }, function(response) {
            //     //console.log(response.farewell);
            //     //onDisconnect();
            // });
        });
    });
}

function onDisconnect() {
    appPort.onDisconnect.addListener(function () {
        if (chrome.runtime.lastError) {
            console.log(chrome.runtime.lastError);
        }
    });
}