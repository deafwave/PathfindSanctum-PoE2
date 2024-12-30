## Profile Content Update
https://poe2db.tw/Trial_of_the_Sekhemas#Affliction
```JavaScript
const allAfflictions = document.querySelectorAll("div#Affliction div.flex-grow-1.ms-2");
let afflictionParsedList = [];

allAfflictions.forEach((oneAffliction) => {
    const splitData = oneAffliction.innerText.split("\n");
    afflictionParsedList.push(`["${splitData[0]}"] = 0, // ${splitData[2]}`);
});

copy(afflictionParsedList);
```