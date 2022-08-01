'use strict';

(async function () {
    await CefSharp.BindObjectAsync('worker');
    document.addEventListener('DOMContentLoaded', async function (ev) {
        let url = ev.path[0].domain;
        console.log(url);
        pageLoadedScheduler(url);
        if(isBalanceOnPage()){
            await worker.SetLogined(true);
        }
        if(document.querySelector("#modal") != null){
            document.querySelector("#modal").style.display = "none";//Убирает модальное окно
        }
        document.querySelector("body").style.overflow = "scroll"
    });
})();

function isBalanceOnPage() {
    return !!document.querySelector('span[data-test-id="QuickCashier-BankRoll"]');
}

async function pageLoadedScheduler(url){
    if(pageLoaded() || await worker.getLogined()){
        worker.domLoaded(url)
        return;
    }
    await sleep(2000);
    await pageLoadedScheduler(url);
}

function pageLoaded(){
    return document.getElementById("main")?.children[0]?.children[1]?.children[0]?.getBoundingClientRect().width !== 0 || !!document.querySelector("input[type='password']")
}

function isLogined(){
    return !!document.querySelector("span[data-test-id=QuickCashier-BankRoll]")
}

async function easyLogin(login,password){
    if(document.getElementById("username") != null && document.getElementById("password") != null){
        let loginElem = document.querySelector("#username > div").children[0];
        let passwordElem = document.querySelector("#password").children[0].children[0];

        if(!loginElem){
            console.log("нет поля с логином")
            return false;
        }

        if(!passwordElem){
            console.log("нет поля с паролем")
            return false;
        }

        loginElem.focus();
        simulate(loginElem,login);
        await sleep(2000);
        passwordElem.focus();
        simulate(passwordElem,password);
        await sleep(500);


        var buttonLogin = Array.from(document.getElementsByTagName("button")).filter(e => e.textContent.toLowerCase() === "вход" || e.textContent.toLowerCase() === "log in")[0];

        if(!buttonLogin){
            console.log("нет кнопки для входа")
            return false;
        }
        buttonLogin.click();

    }else {
        let loginBoxInView = await asyncCallbackLong(()=>{
            return !!document.querySelector("[name=username]");
        },1000,10);
        let loginBox = document.querySelector("[name=username]");
        let passwordBox = document.querySelector("[name=password]");

        simulate(loginBox,login);
        await sleep(2000);
        passwordBox.focus();
        simulate(passwordBox,password);
        await sleep(500);

        let buttonSubmit = window.document.querySelector('button[type=submit]');

        let buttonSubmitInView = await asyncCallbackLong(()=>{
            return !window.document.querySelector('button[type=submit]').attributes.getNamedItem("disabled");
        },1000,10);
        buttonSubmit.click();
    }
    
    let googleKeyRegex = /\bk=(\w*)&\b/;

    
    let capchaExists = await asyncCallbackLong(() => !!document.querySelector("iframe[title='reCAPTCHA']"), 1000, 5)
    let frame = document.querySelector("iframe[title='reCAPTCHA']");


    if (capchaExists) {
        let googleKey = googleKeyRegex.exec(frame.src)[1];
        let result = await worker.getGoogleCapcha(googleKey, window.location.href);

        setCapcha(___grecaptcha_cfg.clients[0], result);
        document.querySelector('button[type=submit]').click();
    }
    return true;
}

function getAuthInfo(){
    // все для авторизации
    let localStorageItems = JSON.parse(localStorage.getItem('Main:User'));
    let xApiKey = globalConfig._config.get("app.api.haywire.apiKey");
    let xSession = localStorageItems.token;
    let xDeviceUuid = localStorageItems.uuid;
    
    return {xApiKey, xSession, xDeviceUuid};
}

function simulate(elem, newValue) {
    let oldValue = elem.value;

    let event = new Event('input', { bubbles: true });
    event.simulated = true;

    elem.value = newValue;

    let tracker = elem._valueTracker;
    if (tracker) {
        tracker.setValue(oldValue);
    }

    elem.dispatchEvent(new KeyboardEvent('keydown', { bubbles: true,cancelable: true}));
    elem.dispatchEvent(new KeyboardEvent('keypress', { bubbles: true,cancelable: true }));
    elem.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true,cancelable: true}));
    elem.dispatchEvent(new KeyboardEvent('click', { bubbles: true,cancelable: true}));
    elem.dispatchEvent(new KeyboardEvent('mousemove', { bubbles: true,cancelable: true}));
    elem.dispatchEvent(new Event('input', { bubbles: true,cancelable: true}));
    elem.dispatchEvent(new Event('change', { bubbles: true,cancelable: true}));
}

function setCapcha(fClient, value) {
    for (let key of Object.keys(fClient)) {
        if (fClient[key]) {
            for (let k of Object.keys(fClient[key])) {
                if (fClient[key][k].hasOwnProperty('callback')) {
                    fClient[key][k].callback(value)
                    return
                }
            }
        }
    }
}

let converterSportId = {
    1:"Бейсбол",
    2:"Баскетбол",
    4:"Футзал",
    5:"Гандбол",
    6:"Хоккей",
    7:"Футбол",
    8:"Теннис",
    9:"Волейбол",
    10:"Футбол (американский)",
    11:"Снукер",
    12:"Дартс",
    13:"Настольный теннис",
    14:"Бадминтон",
    15:"Регби-Лига",
    16:"Водное поло",
    17:"Хоккей с мячом",
    18:"Боевые искусства",
    19:"Хоккей на траве",
    20:"Австралийский футбол",
    21:"Киберспорт",
    22:"Шахматы",
    23:"Гэл.спорт",
    24:"Крикет",
    25:"Формула 1",
    29:"Пляжный волейбол",
    30:"Скачки",
    31:"Биатлон",
    32:"Керлинг",
    33:"Сквош",
    34:"Нетбол",
    35:"Пляжный футболл",
    36:"Флорбол",
    37:"Херлинг",
    38:"Кунг волейбол",
    39:"Киберфутбол",
    40:"Киберхоккей",
    41:"Кибербаскетбол",
    42:"Кибертеннис",
    43:"Регби-Союз",
    44:"Бокс",
    45:"MMA"
}

async function openEvent(eventId,sportId){
    let sportName = converterSportId[sportId];
    if(!sportName){
        console.log("Не смог распарсить название вида спорта");
        return;
    }
    let centerLive = document.querySelector("a[href='/ru/live/overview']");//кнопка Центр Live
    if(!centerLive){
        centerLive = document.querySelector("a[href='http://www.pinnacle.com/ru/live']")
        centerLive.click();
        await sleep(2000)
    }else {
        let btnDisabled = centerLive.classList.contains("undefined")
        if(btnDisabled){
            centerLive.click();
        }
        await sleep(600)
    }

    if(window.location.href !== 'https://www.pinnacle.com/ru/live/overview'){
        console.log("Не смогли перейти в линию событий по причине блокировки провайдером");
        //worker.notificateInBrowserHtml("<h1>Right Click on mouse and choose back</h1>");
        //нужно остановить работу пинакла
    }
    
    let sportTabs = Array.from(document.getElementsByClassName("style_tabLabel__2eazJ"))
    sportTabs.forEach(function (e) {
        console.log(e.textContent)
        if(e.textContent.toLowerCase() === sportName.toLowerCase()){
            e.parentElement.parentElement.click()
        }
    })
    let choosedSport = Array.from(document.getElementsByClassName("tabItem")).map(tab => tab.attributes["data-selected"].value === "true" ? tab.textContent : "")
        .filter(e => e !== "")?.[0]
    if(!choosedSport){
        console.log("не смог найти выбранный спорт")
        return false;
    }
    let clickedOnTrueSport = sportName.toLowerCase() === choosedSport.toLowerCase();
    if(!clickedOnTrueSport){
        console.log("Кликнул не на правильный спорт")
        return false;
    }
    // let eventsLoaded = await asyncCallbackLong(()=>{
    //     return document.querySelector("#root > div > div.style_container__3Xbio > main > div > div > div.style_fixtureGrid__1FBUt").children.length > 1;
    // },500,10);
    await sleep(1500);
    // if(!eventsLoaded){
    //     console.log("Не смог загрузить события для текущего спорта")
    //     return false
    // }
    
    let allEvents = Array.from(document.getElementsByClassName("style_fixtureGrid__1FBUt")[0].children)
    let choosingEvent = allEvents.map(ev => {
        let instance = reactInstance(ev);
        let marketId = instance?.lastEffect?.return?.return?.memoizedProps?.marketId 
            ?? instance?.lastEffect?.return?.return?.memoizedProps?.market?.id 
            ?? instance?.lastEffect?.return?.return?.memoizedProps?.matchup?.id
            ?? instance?.return?.memoizedProps?.market?.id
            ?? instance?.return?.memoizedProps?.market?.marketId
        if(!marketId)
            return "";
        marketId = marketId.toString();
        let chunkMarketId = marketId;
        if(marketId.indexOf("%") !== -1){
            chunkMarketId = marketId.substr(0,marketId.indexOf("%"))
        }
        return chunkMarketId === eventId ? ev : "";
    });
    let event = choosingEvent.filter(elem => elem !== "")[0]
    if(!event){
        console.log("Не нашёл событие")
        return false
    }
    
    let clickableBtn = event.children[0].children[0].children[1].children[0].children[0]
    clickableBtn.click();

    await sleep(2000);

    let matchupsLoaded = await asyncCallbackLong(()=>{
        return !document.getElementById("multiview-matchups")?.children[0]?.children[1]?.children[0];
    },1000,5);

    let matchupItems = document.getElementById("multiview-matchups");
    if(matchupItems.children.length > 1){
        for(let i = 1;i < matchupItems.children.length;i++){
            matchupItems.children[i].remove()
            fireEvent(document.body,"click")
            fireEvent(document.body,"mousemove")
        }
        await sleep(500);
        for(let i = 1;i < matchupItems.children.length;i++){
            matchupItems.children[i].remove()
            fireEvent(document.body,"click")
            fireEvent(document.body,"mousemove")
        }
    }
    
    if(!matchupsLoaded){
        console.log("Не смог загрузить текущее событие")
    }
    let allMarkets = Array.from(document.getElementById("multiview-matchups").children[0].getElementsByClassName("market-btn"))
    allMarkets.forEach(function (market) {
        
    });
}

async function findStakeAndClick(betId){
    let clearBtn = document.querySelector('div[data-test-id="Betslip-RemoveAllButton"],div[data-test-id="Betslip-SelectionDetails"] button');
    let isHaveStake = isInView(clearBtn);
    if(isHaveStake){
        console.log('необходимо очистить купон');
        if (window.document.querySelector('div[data-test-id="Betslip-RemoveAllButton"]')) {
            clearBtn.click();
            let isLoadConfirmBtn = await asyncCallbackLong(() => {
                return isInView(document.querySelector('button[data-test-id="Betslip-RemoveAllModal-ConfirmButton"]'));
            },1000,10);
            if (!isLoadConfirmBtn) {
                console.log('Кнопка подтверждения так и не появилась');
                return false;
            }
            document.querySelector('button[data-test-id="Betslip-RemoveAllModal-ConfirmButton"]').click();
        }else {
            clearBtn.click();
        }
        let isCleared = await asyncCallbackLong(() => {
            return isInView(window.document.querySelector('div[data-test-id="betslip-empty"]'));
        },1000,10);
        if (!isCleared) {
            console.log('купон так и не очистился');
            return false;
        }
    }

    let btnAllStakes = document.getElementById("all");//кнопка чтобы все ставки отобразились
    btnAllStakes.click();
    betId = betId.substr(betId.indexOf("/") + 1,10);
    
    //Для позитива
    // let partials = betId.split("_");
    //
    // let key = partials[1];//'s;0;m'
    // let parameter = null;
    // let title = null;
    // if(partials.length > 3){
    //     parameter = partials[2];//over
    //     title = partials[3];//16.5
    // }else if(partials.length === 3){
    //     parameter = partials[partials.length-1];
    // }
    // parameter = correctParameter(parameter)

    
    let allStakes = Array.from(document.querySelectorAll("button[class^=market-btn]"));
    //let choosedStakes = allStakes.map(stake => reactInstance(stake).return.memoizedProps.market.key === key ? stake:null).filter(stake => stake != null);//поиск для позитива

    let choosedStakes = allStakes.map(function (st){
        let instance = reactInstance(st);
        let firstReturn = instance?.return;
        let secondReturn = firstReturn?.return;
        let memoizedProps = secondReturn?.memoizedProps;
        let market = memoizedProps?.market
        let version = market?.version;
        return version === betId ? st : null;
    }).filter(stak => stak != null);               
    
    let stake = null;
    
    stake = choosedStakes[0];
    
    //ДЛЯ ПОЗИТИВА
    // if(choosedStakes.length === 0){
    //     console.log("не смогли найти ставку")
    //     return false;
    // }
    // if(choosedStakes.length > 1){
    //     console.log("Нашёл много ставок " + choosedStakes.length);
    //     stake = choosedStakes.find(stake => stake.title === parameter != null && title != null ? parameter + title:parameter != null?parameter:"");
    // }else if(choosedStakes.length === 1){
    //     stake = choosedStakes[0];
    // }
    // if(!stake){
    //     console.log("Не смог найти ставку")
    //     return false;
    // }
    
    if(!stake){
        console.log("Ставки не нашёл")
        return false;
    }
    stake.click();

    let stakeInCoupon = await asyncCallbackLong(()=>{
        return !document.querySelector("#betslip-empty");
    },1000,10);
    if(!stakeInCoupon){
        console.log("ставка не в купоне")
        return false;
    }
    console.log("Нажал на ставку в Пинакле");
    return true;
}

function correctParameter(postiveBetParameters)
{
    if (postiveBetParameters === '1')
    {
        return 'К1';
    }
    if (postiveBetParameters === '2')
    {
        return 'К2';
    }
    return postiveBetParameters.replace('/ 1', '/ К1').replace('/ 2', '/ К2').replace('under', 'Меньше')
        .replace('over', 'Больше').replace('over', 'Больше').replace('even', 'Четное').replace('odd', 'Нечетное').replace(' 0.00', ' 0').replace('yes', 'Да').replace('no', 'Нет');
}


function reactInstance(element) {
    let s = Object.keys(element).find(function (k) {
        return ~k.indexOf("__reactInternalInstance")
    });
    if (s === undefined) {
        return undefined;
    }
    return element[s];
}

async function sleep(msec) {
    return new Promise(resolve => window.setTimeout(resolve, msec));
}

async function asyncCallbackLong(callbackBoolean, sleepTime, tryCount) {
    var tryCounter = 0;
    while (tryCounter < tryCount) {
        if (callbackBoolean()) {
            return true;
        }
        tryCounter++;
        await sleep(sleepTime);
    }
    return false;
}

function isInView(element) {
    if (!element) return false;
    return element.getBoundingClientRect().width !== 0;
}

function fireEvent(element, event) {
    var evt = window.document.createEvent('HTMLEvents');
    evt.initEvent(event, true, true);
    element.dispatchEvent(evt);
}