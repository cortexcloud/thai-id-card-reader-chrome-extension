
console.log('Background script is started.')

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


/*
On startup, connect to the "co.cortexcloud.thai_id_card_reader" app.
*/
var port = chrome.runtime.connectNative("co.cortexcloud.thai_id_card_reader_console_app");
var responseData = null;

/*
Listen for messages from the app.
*/
port.onMessage.addListener((resp) => {
    console.log("Received: " + JSON.stringify(resp));

    chrome.tabs.query({active: true, currentWindow: true}, function(tabs) {
        chrome.tabs.sendMessage(tabs[0].id,{ message : resp });

        // chrome.tabs.sendMessage(tabs[0].id,{ message : resp }, function(response) {
        //     //console.log(response.farewell);

        //     //onDisconnect();
        // });
    });

  
});


function onDisconnect() {
    port.onDisconnect.addListener(function () {
        if (chrome.runtime.lastError) {
            console.log(chrome.runtime.lastError);
        }
    });
}