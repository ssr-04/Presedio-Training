const staticData = ["Doomsday", "Secret wars", "Brand New Day", "Eyes of Wakanda", "Iron Heart"]

const displayData = (data) => {
    const list = document.getElementById("dataList");
    staticData.forEach(item => {
        const li = document.createElement("li");
        li.textContent = item;
        list.appendChild(li);
    })
}

const clearData = () => {
    document.getElementById("dataList").innerHTML = "";
    console.clear();
    console.log("Data cleared.");
}

// Callback implemetation

const getDataWithCallback = (callback) => {
  console.log("Callback: Start fetching data...");
  setTimeout(() => {
    console.log("Callback: Data fetched.");
    callback(staticData);
  }, 500);
}

const loadWithCallback = () => {
  clearData();
  getDataWithCallback((data) => {
    console.log("Callback: Rendering data...");
    displayData(data);
  });
}

// Promise implementation

const getDataWithPromise = () => {
  console.log("Promise: Start fetching data...");
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      console.log("Promise: Data fetched.");
      resolve(staticData);
    }, 500);
  });
}

const loadWithPromise = () => {
  clearData();
  getDataWithPromise().then(data => {
    console.log("Promise: Rendering data...");
    displayData(data);
  });
}

// Async/await Implementation

const getDataAsync = () => {
  return new Promise(resolve => {
    console.log("Async/Await: Start fetching data...");
    setTimeout(() => {
      console.log("Async/Await: Data fetched.");
      resolve(staticData);
    }, 500);
  });
}

async function loadWithAsync() {
  clearData();
  const data = await getDataAsync();
  console.log("Async/Await: Rendering data...");
  displayData(data);
}
