console.log('foreground script is started.')
//var txtResult = document.getElementById('#result');
chrome.runtime.onMessage.addListener(
  function(request, sender, sendResponse) {
      
    console.log(request.message)
    document.getElementById("result").value = JSON.stringify(request.message); 


    //txtResult.value(JSON.stringify(request.message));
    // console.log(sender.tab ?
    //             "from a content script:" + sender.tab.url :
    //             "from the extension");
    // if (request.greeting == "hello")
    // {
    //   sendResponse({farewell: "goodbye"});
    // }
    // else {
    //     console.log(request.message)
    // }
  }
);