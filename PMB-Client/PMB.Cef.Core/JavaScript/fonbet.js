(async function()
{
    await CefSharp.BindObjectAsync('worker');
    document.addEventListener('DOMContentLoaded', function (ev) {
        let url = ev.path[0].domain;
        worker.domLoaded(url)
    });
})();
//CONSTANTS
const COLORS_ENTER_BTN = ["212, 42, 40", "37, 62, 91"]
const COLOR_LIVE_BTN = "246, 248, 249"
const FONBET_DATA_URL = "https://line10w.bkfon-resources.com/events/listBase?lang=ru&scopeMarket=1600"
const KIND_SPORT = "sport"
const KIND_SEGMENT = "segment"
const SPORTS_DICT = {
    "футбол": "football",
    "хоккей": "hockey",
    "баскетбол": "basketball",
    "баскетбол 3x3": "basketball-3x3",
    "волейбол": "volleyball",
    "теннис": "tennis",
    "настольный теннис": "table-tennis",
    "пляжный футбол": "beach-football",
    "хоккей на траве": "field-hockey",
    "флорбол": "floorball",
    "регби": "rugby",
    "австралийский футбол": "australian-football",
    "бейсбол": "baseball"
}
const GAMES_FOR_EXCEPTION_IN_TENNIS = [11,13,23,36]

//Авторизация
async function easyLogin(login,pass){
    let loginBtn = null;//Кнопка Вход
    let loginOuterContainer = null;//Модальное окно для авторизации
    let loginOuterContainerLoaded = await asyncCallbackLong(()=>{
        if(isInView(document.querySelector("div.authorization__inner"))){
            loginOuterContainer = document.querySelector("div.authorization__inner");
            return true;
        }else{
            return false;
        }
    },300,20);
    if(!loginOuterContainerLoaded){
        let btnLoginLoaded = await asyncCallbackLong(()=>{
            return isInView(document.querySelector("a._reg-btn")) &&
                isInView(document.querySelector("a._login-btn")) &&
                COLORS_ENTER_BTN.some(color => window.getComputedStyle(document.querySelector("a._login-btn")).backgroundColor.includes(color))
        },300,30);
        if(!btnLoginLoaded){
            console.log("Кнопка вход не была загружена")
            return false
        }
        loginBtn = document.querySelector("a._login-btn")
        fireMouseEvent(loginBtn, "mouseover");
        fireMouseEvent(loginBtn, "mousemove");
        loginBtn.click();
        loginOuterContainerLoaded = await asyncCallbackLong(()=>{
            if(isInView(document.querySelector("div.authorization__inner"))){
                loginOuterContainer = document.querySelector("div.authorization__inner");
                return true;
            }else{
                return false;
            }
        },300,20);
    }
    let loginInnerContainerLoaded = await asyncCallbackLong(()=>{
        return isInView(loginOuterContainer.children[1])
    },300,20);
    if(!loginInnerContainerLoaded){
        console.log("Внутренний контейнер для ввода значений логина и пароля не загрузился")
        return false;
    }
    let numberOfAccountAndPassword = loginOuterContainer.querySelectorAll("input");
    let loginContainer = numberOfAccountAndPassword[0]
    let passwordContainer = numberOfAccountAndPassword[1]
    
    simulate(loginContainer,login)
    await sleep(1000);
    simulate(passwordContainer,pass)
    
    let buttonEnter = document.querySelector("button.toolbar__btn");
    buttonEnter.click();
    let notCorrectAuthData = await asyncCallbackLong(()=>{
        return document.querySelector("div[class^=login-form__error]") !== null;
    },200,20);
    if(notCorrectAuthData){
        console.log("Не правильно введены авторизационные данные")
        return false
    }
    return true;
}

//Проверка на логин
async function isLogined() {
    try {
        return window.app.loggedIn();
    } catch (e) {
        console.warn('Ошибка проверки авторизации на фоне ', e);
        return false;
    }
}

//Генерирует ссылку на основе eventId и переходит через location
async function openEvent(eventId){
    let url = location.href;
    let liveBtn = document.querySelector(".menu__link[href='/live/']");
    let choosedLiveBtn = window.getComputedStyle(liveBtn).backgroundColor.includes(COLOR_LIVE_BTN)
    var liveUrl = url.includes("/live")

    if(!liveUrl || !choosedLiveBtn){
        liveBtn.click();
        await sleep(2000);
    }
    const eventUrl = await getUrlByEvent(eventId)
    if(!eventUrl){
        console.log("Не смог распарсить eventUrl")
        return false
    }
    location.href = eventUrl
    return true
    
    //TODO:Поиск ставки через javascript через скролл
    // let blockForScrolling = document.querySelectorAll("div[class*=custom-scrollbar-area__view-port] > div")[4]
    // let foundEventElement = null
    // let currentTime = 0
    // let LIMIT_WAIT_SECONDS = 10
    // let timer = setInterval(()=>{
    //     currentTime++
    // },1000)
    // while(!foundEventElement && currentTime < LIMIT_WAIT_SECONDS){
    //     let mainBlockItems = document.querySelectorAll("div[class*='sport-section-virtual-list']")[0]
    //     const mixItems = Array.from(mainBlockItems.children)
    //     foundEventElement = mixItems.filter(item => findReactInstance(item).key?.split("_")[2] == eventId)[0]
    //     blockForScrolling.scrollBy(0,100)
    //     await sleep(500)
    // }
    // clearInterval(timer)
    // if(!foundEventElement){
    //     console.log("Событие не найдено")
    //     return false
    // }
    // return true
}

//Проверяет,что центральный блок на странице и любая ставка прогрузились
function mainBlockEventAndAnyStakeExist(){
    const middleBlockOnPageEventRect = document.querySelectorAll("div[class*=sport-filter-layout]")[1]?.getBoundingClientRect()
    const middleBlockOnPageEventExists = !!(middleBlockOnPageEventRect && middleBlockOnPageEventRect.width !== 0 && middleBlockOnPageEventRect.height !== 0 && middleBlockOnPageEventRect.y !== 0 && middleBlockOnPageEventRect.x !== 0)
    const anyStakeRect = document.querySelector("div[class*='cell-wrap']")?.getBoundingClientRect()
    const anyStakeExists = !!(anyStakeRect && anyStakeRect.width !== 0 && anyStakeRect.height !== 0 && anyStakeRect.y !== 0 && anyStakeRect.x !== 0)
    return middleBlockOnPageEventExists && anyStakeExists
}

//Делает запрос к фонбету,берёт события и спорты
//Возвращает URL Текущего события
//Если одно из аргументов не корректен => возвращает пустую строку,поэтому валидировать нужно через двойное равно ==
async function getUrlByEvent(eventId){
    if(!eventId){
        console.log("EventId пустой")
        return ""
    }
    const eventsSportDataResponse = await fetch(FONBET_DATA_URL)
    const data = await eventsSportDataResponse.json()
    const events = data.events
    const sports = data.sports
    
    let foundEvent = events.filter(event => event.id == eventId)[0]
    if(!foundEvent.sportId){
        console.log("У текущего события не нашёл eventId")
        return ""
    }
    const sportId = foundEvent.sportId
    if(!sportId){
        console.log("У текущего события не нашёл sportId")
        return ""
    }
    
    let sportName = null;
    sportName = sports?.filter(sport => sport.kind == KIND_SPORT && sport.id == sportId)[0]?.name
    
    if(!sportName){
        let parentIdForSegmentAndSportId = sports?.filter(sport => sport.kind == KIND_SEGMENT && sport.id == sportId)[0]?.parentId
        sportName = sports?.filter(ev => ev.id == parentIdForSegmentAndSportId)[0]?.name
    }
    sportName = sportName.toLowerCase()
    let sportNameLatinica = SPORTS_DICT[sportName]
    if(!sportNameLatinica){
        console.log("Не смог найти в словаре Спортов текущий спорт " + sportName)
        return ""
    }
    let originUrl = location.origin
    return originUrl + "/live/" + sportNameLatinica + "/" + sportId + "/" + eventId  
}

//Поиск ставки и клик на ставку
async function findStakeAndClick(betId) {
    patchline21();
    try {
        let betIdSplit = betId.split('|');
        let eventId = parseInt(betIdSplit[0]);
        let factorId = parseInt(betIdSplit[1]);
        let place = "live";
        let event = null;
        let factor = null;
        try {
            event = getEvent(eventId)
            factor = getFactor(event, factorId);
        } catch (e) {
            console.log("Не нашёл при первом поиске eventId и factorId");
        }
        if (!event) {
            let eventPromise = new Promise((resolve, reject) => {
                window.app.couponManager.requestEventFactors([{
                    eventId: eventId,
                    factorId: factorId,
                    place: place
                }], function (a) {
                    resolve(a)
                }, function (b) {
                    resolve([]);
                    console.log("cannot fetchEventFactors " + b)
                })
            });
            let events = await Promise.race([eventPromise, sleep(500)]);
            if (typeof events == "undefined") {
                console.log("Не можем открыть купон, глюк в сайте")
                return openCoupon();
            }
            if (events.length === 0 || !events[0] || !events[0].rootId) {
                console.log('Событие не найдено');
                return false;
            }
            event = events[0];
            let factors = event.factors;
            if (factors.length === 0 || !factors[0]) {
                console.log('Фактор не найден!');
                return false;
            }
            factor = factors[0];
        }

        if (!factor || factor.blocked) {
            console.log('Ставка недоступна!');
            return false;
        }
        await clearCoupon();
        window.app.couponManager.newCoupon.newAddStake(place, place, event.rootId, event.id, factor.id, factor.p ?? factor.v);
        try {
            await applyCoefChange();
        } catch (e) {
        }
        try {
            await applyExpandedCoupon();
        } catch (e) {
        }

        let isLoadMinSum = await asyncCallbackHot(checkLoadMinSumm);
        if (!isLoadMinSum) {
            console.log('Фон не загрузил минимальную ставку');
            return false;
        }

        let isLoadMaxSum = await asyncCallbackHot(checkLoadMinSumm);
        if (!isLoadMaxSum) {
            console.log('Фон не загрузил максимальную ставку');
            return false;
        }
        
        return true;
    } catch (e) {
        //throw e;
        return false;
    } finally {
        //isOpenStakeNow = false;
    }
    return false;
}

//Установить сумму ставки
async function setStakeSum(sum) {
    let newSum = Math.floor(sum);
    try {
        let selector = 'input[class^="sum-panel__input"]';
        var input = document.querySelector(selector);
        createInputReactEvent(input, newSum);

        console.log('Вписали сумму ' + newSum);
        console.log('Сумма в купоне ' + input.value);

        return true;
    } catch (e) {
        if (e.message.indexOf("Can't enqueue an asap callback in a context") !== -1) {
            return true;
        }
        window.console.log('setStakeSumm error: ' + e);
        return false;
    }
}

//Заключить пари
async function doStake() {
    if (!await getStakeEnabled()) {
        console.log('Ставка стала недоступной');
        return false;
    }
    let amount = await getStakeAmount();
    if (amount === 0) {
        console.log('Сумма в купоне 0');
        return false;
    }
    // if (scope.lastParameter !== await getStakeParameter()) {
    //     writeLog('Параметр изменился');
    //     return false;
    // }
    // if (scope.lastCoef !== await getStakeCoef()) {
    //     writeLog('Коэффициент изменился');
    //     return false;
    // }
    var currentSumm = await getStakeAmount();
    if (isNaN(currentSumm)) {
        console.log('Текущая сумма не определена!');
        return false;
    }
    if (currentSumm === 0) {
        console.log('Текущая сумма = 0!');
        return false;
    }
    if (currentSumm < await getStakeMin()) {
        console.log('Сумма в купоне меньше минимальной!');
        return false;
    }
    //scope.lastBalance = await getUserBalanceNew();
    return await doStakeInternal();
}

async function getStakeEnabled() {
    try {
        let stake = cStake();
        if (!stake) {
            console.log("Ставки нет в купоне");
            return false;
        }
        return (stake.available === true &&
            stake.blocked !== true);
    } catch (e) {
        window.console.log('Ошибка проверки доступности');
        return false;
    }
}

//Логика проставления ставки
async function doStakeInternal() {
    let selectorAgreeBtn = 'div[class*="button-accept--"]';
    let agreeButton = window.document.querySelector(selectorAgreeBtn);
    if (isInView(agreeButton)) {
        agreeButton.click();
        // console.log('Ставку не поставили. Были изменения');
        // return false;
    }

    let button = window.document.querySelector('div[class*="normal-bet"]');
    if (!button) {
        console.log('Нет кнопки "сделать ставку"');
        return false;
    }
    if (button.classList.value.indexOf('disabled') !== -1) {
        console.log('Кнопка "сделать ставку" - недоступна');
        return false;
    }
    if (window.app.couponManager.newCoupon.__proto__.cloneCouponToPlace !== injectCoupon) {
        window.cloneCouponToPlacePtr = window.app.couponManager.newCoupon.__proto__.cloneCouponToPlace;
        window.app.couponManager.newCoupon.__proto__.cloneCouponToPlace = injectCoupon;
    }
    let betButton = window.document.querySelector('div[class*="place-button-"] > div[class*="button--"]');
    if (!betButton) {
        console.log("Нет кнопки нажать ставку");
        return false;
    }
    try {
        await applyCoefChange();
    } catch (e) {
    }
    try {
        await applyExpandedCoupon();
    } catch (e) {
    }
    let doStakeBtn = window.document.querySelector('div[class*="place-button-"] > div[class*="button--"]');
    doStakeBtn.click();
    try {
        await applyCoefChange();
    } catch (e) {
    }
    try {
        await applyExpandedCoupon();
    } catch (e) {
    }
    console.log("Нажали сделать ставку!");
    await sleep(20);
    return true;
}

//Значение минимальной ставки
async function getStakeMin() {
    try {
        let res = parseFloat(window.app.couponManager.newCoupon.amountMin);
        if (res === 0 || isNaN(res)) {
            console.log('amountMin==0');
        }
        return res;
    } catch (e) {
        console.log('Ошибка получения минимального значения ставки', e);
        return 0;
    }
}

//Значение максимальной ставки
async function getStakeMax() {
    try {
        let res = parseFloat(window.app.couponManager.newCoupon.amountMax);
        if (res === 0 || isNaN(res)) {
            console.log('amountMax==0');
        }
        return res;
    } catch (e) {
        console.log('Ошибка получения максимального значения ставки', e);
        return 0;
    }
}

//Принятия изменения коэффициентов
async function applyCoefChange() {
    let savedSet = window.app.headerApi.settings();
    let set = {
        takeChangedTotals: false,
        takeChangedBets: false,
        takeUpBets: false
    }
    set.takeChangedTotals = false;

    // if (await worker.GetCoefAcceptChange() === 'all') {
    //     console.log('Включаем принимать с любыми изменениями кэфа');
    //     set.takeChangedBets = true;
    //     set.takeUpBets = false;
    // } else if (await worker.GetCoefAcceptChange() === 'up') {
    //     writeLog('Включаем принимать с повышением кэфа');
    //     set.takeChangedBets = false;
    //     set.takeUpBets = true;
    // } else if (await worker.GetCoefAcceptChange() === 'none') {
    //     writeLog('Включаем не принимать изменения кэфа');
    //     set.takeChangedBets = false;
    //     set.takeUpBets = false;
    // }
    if (savedSet.takeChangedBets !== set.takeChangedBets || savedSet.takeUpBets !== set.takeUpBets) {
        savedSet.takeChangedBets = set.takeChangedBets;
        savedSet.takeUpBets = set.takeUpBets;
        window.app.settingsApply(set, 'takeUpBets');
    }
}

//Очистка купона
async function clearCoupon() {
    window.app.couponManager.newCoupon.clear();
}

//Сумма, вписанная в купон
async function getStakeAmount() {
    try {
        let rez = parseFloat(window.app.couponManager.newCoupon.amount);
        if (rez === 0 || isNaN(rez)) {
            window.console.warn('amount==0');
        }
        return rez;
    } catch (e) {
        window.console.warn('Ошибка получения суммы', e);
        return 0;
    }
}

//Информация по купону
function newCoupon() {
    return window?.app?.couponManager?.newCoupon;
}

//Информация о текущей ставке
function cStake() {
    return newCoupon()?.stakes[0];
}

//Коэффициент в купоне
async function getStakeCoef() {
    let stake = cStake();
    if (!stake) {
        return 1;
    }
    if (stake.vNew)
        return parseFloat(stake.vNew);
    return parseFloat(stake.v);
}

//Загружена ли страница
async function IsPageLoaded() {
    let loadedHeader = await asyncCallbackHot(() => {
        return !!isInView(document.querySelector("#headerContainer"));
    });
    let main = document.querySelector("#main");
    return main && main.childElementCount > 0 && loadedHeader && window.app && window.app.lineManager && app._ready;
}

//Проверка Геймов в теннисе
async function checkGameInTennis(eventId){
    try{
        const eventsSportDataResponse = await fetch(FONBET_DATA_URL)
        const data = await eventsSportDataResponse.json()
        const eventMiscs = data.eventMiscs
        const event = eventMiscs?.filter(event => event.id == eventId)[0]
        const splittedComment = event.comment.replace("(","").replace(")","").split(" ")//['6-4', '3-6', '6-2', '6-5']
        const actualPartiya = splittedComment[splittedComment.length - 1]//6-5
        const splittedActualPartiya = actualPartiya.split("-")
        const currentGame = parseInt(splittedActualPartiya[0]) + parseInt(splittedActualPartiya[1]) + 1
        return !GAMES_FOR_EXCEPTION_IN_TENNIS.some(gameValue => gameValue == currentGame)
    }catch(e){
        return false;
    }
}

//Парсит только время,но возвращает полноценный объект,который не инициализирован
//На данный момент мы ничего не используем кроме времени
function parseMatchData(){
    var time = document.querySelector("div[class*=ev-live-time]").textContent//'5:59'
    return {
        AdditionalData: "",
        AdditionalDataSet: "",
        AdditionalDataGame: "",
        AdditionalDataScore: "",
        Time: {
            MainTime: time ?? "",
            AdditionalTime: ""
        }
    };
}

//Баланс на конторе
function parseBalance() {
    const balanceSelector = document.querySelector('.header__login-balance')
    const currencySelector = document.querySelector('.header__login-currency')
    let amount = null
    let currency = null;
    try {
        amount = parseFloat(balanceSelector.innerText.replace(' ', '').trim())
        currency = currencySelector.innerText.trim()
    } catch (ex) {
        console.log('Не смогли спарсить баланс' + ex);
        return 0;
    }
    if(!amount || !currency){
        console.log("не смогли спарсить баланс")
        return 0;
    }

    return {
        Amount: amount,
        Currency: currency
    }
}

function injectCoupon() {
    const currentCoupon = window.cloneCouponToPlacePtr.apply(this, arguments);
    //window.console.dir(scope.currentCuppon);
    //window.console.dir('ставим');
    return currentCoupon;
}

function checkLoadMinSumm() {
    try {
        if (window.app.couponManager.newCoupon.amountMin === 0 ||
            isNaN(window.app.couponManager.newCoupon.amountMin) ||
            !window.app.couponManager.newCoupon.amountMin) {
            console.warn('Минставка еще не загрузилась');
            return false;
        }

        return true;
    } catch (e) {
        console.warn('Ошибка загрузки минставки', e);
        return false;
    }
}

async function applyExpandedCoupon() {
    let savedSet = window.app.headerApi.settings();
    if (savedSet.isCompact) {
        window.app.settingsApply({isCompact: false}, 'isCompact');
    }
}

async function getUserBalanceNew() {
    try {
        return parseFloat(window.document.querySelector('.header__login-balance').innerText.replace(' ', '').trim());
    } catch (ex) {
        console.log('Не смогли спарсить баланс' + ex);
        return -1;
    }
}

async function updateBalance() {
    let balance = await getUserBalanceNew();
    return balance;
}

async function openCoupon() {
    let islogined = await isLogined();
    if (!islogined) {
        return false;
    }
    await clearCoupon();
    await updateBalance();
    if (typeof beforeOpenCoupon !== "undefined") {
        let passed = await beforeOpenCoupon();
        if (!passed) {
            console.log("Купон не открываем");
            return false;
        }
    }
    //TODO:Разобраться с данным блоком кода
    return true
    //let isLive = await isLiveEvent();
    // let canOpenPrematch = typeof openPrematchCouponInternal !== "undefined" && !isLive;
    // let opened = canOpenPrematch ? await openPrematchCouponInternal() : await openCouponInternal();
    // if (!opened) {
    //     console.log("Купон так и не открылся!");
    //     return
    // }
    // if (!await checkBet()) {
    //     console.log("Ставка не прошла проверку");
    //     return
    // }
}

async function checkBet() {
    if (app.session && app.session.limitGroup) {
        let lg = window.app.session.limitGroup
        if (lg === 2 || lg === 11 || lg === 4) {
            console.log('Аккаунт фонбет порезан до 100-300');
        }
    }
    let stake = app.couponManager.newCoupon.stakes[0];
    if (!stake || !stake.eventName) {
        console.log("ставка не найдена")
        return false;
    }
    let eventname = stake.eventName.toLowerCase();
    let bn = (await worker.GetBetName()).toLowerCase();
    let stakename = stake.stakeName.toLowerCase();
    let factor = parseInt(stake.factorId);

    if (!checkBetPeriod(stake, bn)) {
        console.log('Не прошла проверка на период');
        return false;
    }

    if (!checkWin(bn, eventname, stakename, factor)) {
        console.log('Не прошла проверка на выигрыш');
        return false;
    }

    return true;
}

async function beforeOpenCoupon() {
    let loadedHeader = await asyncCallbackHot(() => {
        return !!isInView(document.querySelector("#headerContainer"));
    })
    if (location.protocol === "data:" || !loadedHeader) {
        console.log("Перезагружаем страницу");
        return false;
    }

    let liveBtn = document.querySelector(".menu__link[href='/live/']");
    if (liveBtn && liveBtn.parentElement && !liveBtn.parentElement.classList.contains("_state_active")) {
        liveBtn.click();
        await sleep(2000);
    }
    let openedEvent = document.querySelector("[class^=event-view-control]")
    if (!openedEvent) {
        let events = document.querySelectorAll("div.table__match-title-text:not(.no_link)")
        if (events.length > 0) {
            events[0].click();
            await sleep(500);
        }
    }
    return true;
}

function getEvent(eventId) {
    return window.app.lineManager.getEvent(eventId);
}

function getFactor(event, factor) {
    if (!event) {
        return null;
    }
    window.console.info('Ищем фактор ' + factor);
    for (var key in event._factors._factors) {
        if (key === factor.toString()) {
            return event._factors._factors[key];
        }
    }
    return null;
}

function patchline21() {
    if (!window.app) {
        return;
    }
    let orig = app.__proto__.serverNameByServiceName;
    if (!orig) {
        return;
    }

    Object.defineProperty(app.__proto__, "serverNameByServiceName", {
        value: function () {
            let res = orig.apply(this, arguments);
            if (res && res.includes("line21.")) {
                return res.replace("line21.", "line11.")
            }
            if (res && res.includes("line20.")) {
                return res.replace("line20.", "line11.")
            }
            return res;
        }
    });
}

function findReactInstance(elem){
    let propName = null
    for(let i in elem){
        if(i.toLowerCase().indexOf("reactinternal") !== -1){
            propName = i
            break
        }
    }
    return elem[propName]
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

async function asyncCallbackHot(callbackBoolean) {
    return await asyncCallbackLong(callbackBoolean, 5, 2000);
}

function isInView(element) {
    if (!element) return false;
    return element.getBoundingClientRect().width !== 0;
}

function fireMouseEvent(element, event) {
    var evt = document.createEvent('MouseEvents');
    evt.initEvent(event, true, true);
    element.dispatchEvent(evt);
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

    elem.dispatchEvent(new KeyboardEvent('keydown', { bubbles: true }));
    elem.dispatchEvent(new KeyboardEvent('keypress', { bubbles: true }));
    elem.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true }));
    elem.dispatchEvent(new Event('input', { bubbles: true }));
    elem.dispatchEvent(new Event('change', { bubbles: true }));
}

async function sleep(msec) {
    return new Promise(resolve => setTimeout(resolve, msec));
}

function createInputReactEvent(element, newValue) {
    element.value = newValue;
    let fakeEvent = {
        target: element
    }
    let funcChangePointer = getFuncPointer(element);

    if (funcChangePointer) {
        let delegateName = 'memoizedProps';
        if (!funcChangePointer[delegateName])
            delegateName = '_wrapperState';

        funcChangePointer[delegateName].onChange(fakeEvent);
        window.console.log('Событие иницированно');
    } else {
        window.console.log('Указатель на функцию не найден');
    }
}

function getFuncPointer(element) {
    var reactName = null;
    for (var key in element) {
        if (key && key.toString().indexOf('__reactInternalInsta') !== -1) {
            reactName = key.toString();
            break;
        }
    }
    if (!reactName)
        return null;
    return element[reactName];
}