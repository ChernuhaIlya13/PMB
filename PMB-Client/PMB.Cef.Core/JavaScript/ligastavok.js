(async function()
{
    await CefSharp.BindObjectAsync('worker');
    
    document.addEventListener('DOMContentLoaded', function (ev) {
        let url = ev.path[0].domain;
        console.log(url)
        worker.domLoaded(url)
    });
})();

async function easyLogin(login,password){
    //worker.logi('Login в лиге старт');

    await asyncCallbackLong(() => !!document.querySelector("#header-sign-in > span > span"), 500, 20)
    
    let balanceBk = document.getElementById("header__wallets-dropdown");
    if(balanceBk){
        worker.domLoaded("www.ligastavok.ru");
        return true;
    }

    let btnEntry = await asyncCallbackLong(()=>{
        return !!document.querySelector("div[id='auth-panel']").children[0];
    },500,20);
    
    if(btnEntry){
        document.querySelector("div[id='auth-panel']").children[0].click();
    }
    
    let headerPanelCheck = await asyncCallbackLong(()=>{
        return !!document.querySelector("#app > div.modal-default-f42cf0.modal-c81b97.modal-default_entered > div:nth-child(2) > div.modal-default__content-f4bc6e.modal__content-0ce3c7.modal-default__content_entered-54d310 > header");
    },500,20);

    if(!headerPanelCheck){
        //worker.logi("Не загрузил авторизационное окно");
    }

    let headerPanel = document.querySelector("#app > div.modal-default-f42cf0.modal-c81b97.modal-default_entered > div:nth-child(2) > div.modal-default__content-f4bc6e.modal__content-0ce3c7.modal-default__content_entered-54d310 > header").nextElementSibling;
    
    let forPhone = headerPanel.children[0].children[0];
    
    let forEmail = headerPanel.children[0].children[1];
    
    let loginEl;
    
    let passwordEl;
    
    if(login.includes("@")){
    
    forEmail.click();
    
    loginEl = document.querySelector("input[name='email']");
    
    passwordEl = document.querySelector("input[name='password']");
    }else{
    forPhone.click();
    
    login = "(" + login.substring(1, 4) + ")" + login.substring(4, 7) + "-" + login.substring(7,9) + "-" + login.substring(9);
    
    loginEl = document.querySelector("input[name='mobilePhone']");
    
    passwordEl = document.querySelector("input[name='password']");
    }
    
    simulate(loginEl,login);
    
    simulate(passwordEl,password);

    await sleep(2000)
    
    let buttonSubmit = document.querySelector("button[type='submit']");
    
    
    buttonSubmit.click();
    worker.domLoaded("www.ligastavok.ru");
    return true;
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

function parseMatchData(){
    let matchData = null;
    if(document.getElementById("event__header").children[0].children[1].children.length > 1){
        matchData = document.getElementById("event__header").children[0].children[1].lastChild.textContent;
    }else if (document.getElementById("event__header").children[0].children[1].children.length === 1){
        matchData = document.getElementById("event__header").children[0].children[1].firstChild.textContent;
    }
    let partialsMatchData = matchData?.split(", ");
    
    let additionalData = partialsMatchData[0];
    let time = partialsMatchData[1].replace("-я минута","");
    
    let objTime = {
        Time:time,
        AdditionalData:""
    };
    
    return {
        AdditionalData:additionalData,
        Time:objTime
    };
}

async function findStakeAndClick(betId){
    await sleep(1000);
    let betData = betId.split('_');
    let isLoadLine = await asyncCallbackHot(() =>
    {
        return !!document.getElementsByClassName('bui-outcome-87025c')[0];
    });
    if (!isLoadLine)
    {
        //worker.logi("Линия так и не загрузилась в Лигеставок")
        return false;
    }
    if (getStakeCount() !== 0)
    {
        let clearButton = document.getElementById("betslip__button_clear");
        if (!clearButton)
        {
            //worker.logi("Нет кнопки очистить купон в Лигеставок")
            return false;
        }
        clearButton.click();
        let isClearedCoupon = await asyncCallbackHot(() =>
        {
            return getStakeCount() === 0;
        }, 200, 50);
        if (!isClearedCoupon)
        {
            //worker.logi("Корзина так и не очистилась в Лигеставок")
            return false;
        }
    }
    let header = findHeader(betData[0]);
    if (!header)
    {
        //console.log("не смог найти Header")
        //worker.logi("Заголовок не найден")
        return false;
    }
    let marketsBlock = header.nextSibling;
    if (!marketsBlock)
    {
        //worker.logi("Нет блока с маркетами в Лигеставок")
        return false;
    }
    let markets = findMarketBlocks(marketsBlock, betData[1]);
    if (markets.length === 0)
    {
        //worker.logi('Нет маркетов в Лигеставок');
        return false;
    }
    let correctedParameter = correctParameter(betData[2]);
    let outcomeElement = findStake(markets, correctedParameter);
    if (!outcomeElement)
    {
        outcomeElement = findStake(markets, betData[2]);
        if (!outcomeElement)
        {
            //worker.logi('Ставка не найдена в Лигеставок');
            return false;
        }
    }
    outcomeElement.click();
    await sleep(2000);
    let isStakeInBasket = await asyncCallbackHot(() =>
    {
        return getStakeCount() === 1;
    });
    if (!isStakeInBasket)
    {
        //worker.logi("Ставка так и не попала в купон в Лигеставок");
        return false;
    }
    //worker.logi('Поставили ставку в Лигеставок');
    return true;
}
function findStake(markets, searchedOutcome)
{
    for (let market of markets)
    {
        let outcomeElements = Array.from(market.nextSibling.getElementsByClassName('bui-outcome-87025c'));
        let outcomeElement = outcomeElements.find(outcomeElement => {
            let outcomeText = outcomeElement.getElementsByClassName('bui-outcome__title-1ebb32')[0];
            return outcomeText && outcomeText.textContent.replace('чёт', 'чет') === searchedOutcome;
        });
        if (outcomeElement)
        {
            return outcomeElement;
        }
    }
    return null;
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
    return postiveBetParameters.replace('/ 1', '/ К1').replace('/ 2', '/ К2').replace('under', 'мен')
        .replace('over', 'бол').replace('over', 'бол').replace('even', 'чет').replace('odd', 'нечет').replace(' 0.00', ' 0').replace('yes', 'Да').replace('no', 'Нет');
}

function findMarketBlocks(marketsBlock, blockName)
{
    let markets = Array.from(marketsBlock.getElementsByClassName('market__title-0ff163'));
    return markets.filter(market => {
        return market.textContent === blockName;
    });
}

function findHeader(headerName)
{
    let headers = Array.from(document.getElementsByClassName('part__title-702475'));
    return headers.find(header => {
        return header.textContent === headerName;
    });
}

function setStakeSum(sumStake){
    try {
        simulate(document.querySelector('input[class^=betslip__input-]'),sumStake)
        return true;
    } catch (e) {
        //worker.logi("Не смог установить сумму ставки");
        return false;
    }
}

function doStake(){
    try{
        let btn = document.querySelectorAll('[class^=bar__submit-]')[0];
        if (reactEventHandlers(btn).onClick) {
            reactEventHandlers(btn).onClick();
        } else {
            btn.click()
        }
        return true;
    }catch (e) {
        //worker.logi("Не удалось заключить пари")
        return false;
    }
    
}
async function GetOpenStakesInStash(){
    try{
        let allStakesButton = document.querySelector("#header__bets");
        allStakesButton.click();
        await sleep(1000);
        let mainBlockStakes = document.querySelector(".modal-f95a23").children[1].children[1].children[0].children[1].children[0];
        let countStakes = 0;
        for(let i = 0;i < mainBlockStakes.children.length;++i){
            if(mainBlockStakes.children[i].children[2].textContent.toLowerCase().includes("продать")){
                countStakes++;
            }
        }
        return countStakes;
    }catch (e) {
        worker.logi("Не удалось получить все открытые ставки в Лигеставок");
        return null;
    }
}

async function checkWindowResult(){
    try{
        let isActiveProgressDialog = isInView(document.querySelector("[data-type=loading]"));
        worker.logi('Ставка еще ставится на лигеставок? ' + isActiveProgressDialog ? "Да":"Нет");
        var isOpenWindowWithResult = await asyncCallbackLong( () => {
            return isInView(document.querySelector("[data-type=done]"));
        },1000,10);
        if(isOpenWindowWithResult)
            worker.couponWindowResultCallback("ligastavokCouponWindow");
    }catch (e) {
        worker.logi("При проверке окна результата на лигеставок произошла ошибка")
    }
}


async function checkWindowResultTextSuccess() {
    try{
        let stakeCount = await GetOpenStakesInStash();
        if(stakeCount != null){
            worker.logi("Количество открытых ставок сейчас в Лигеставок: " + stakeCount);
        }

        let doneMsg = document.querySelector("[data-type=done]");

        let isPerfectBetStatusText = (doneMsg.textContent === "Пари приняты");

        worker.logi("Текст ставки в конторе Лигаставок:" + doneMsg.textContent);
        if(isPerfectBetStatusText){
            worker.logi('Ставка в Лигеставок успешно принята');
        }
        if(!isPerfectBetStatusText){
            worker.logi("Ошибка при ставке на Лигеставок")
        }
        return isPerfectBetStatusText;
    }catch(ex){
        worker.logi("При проверке на Лигеставок результата окна текста произошла ошибка");
        return false;
    }
}

//Бот элемент на страничке,как в бетинге
function removeLivetex() {//Удалям чатовое окно
    let element = document.querySelector("[class^=livetex]");
    if (element) {
        element.remove();
    }
}

function initBrowser() {//через 500 мс нужно попробовать логин, елси в лгин будет ошибка вывести error. а если тру, то через 3 сек вывзвать метод изшоустаке
    if (!worker.IsShowStake) {
        setTimeout(function () {
            try {
                login();
            } catch (e) {
               //console.log("error ");
            }
        }, 500);
    } else
        setTimeout(showStake, 3000);

}


var checkCounter = 0;
var reloginTry = 0;

var dostaketry = 0;

let maxstaketry = 3;

var candostake = true;

function updateBalance() {//обновление баланса
    if (checkLogin()) {
        try {
            var balance = getBalance();
            var tryCount = 0;
            while (tryCount < 100 && balance === -1) {
                balance = getbalance();
                if (balance !== -1)
                    break;
                balance = getbalance();
                tryCount++;
                console.log('попытка баланса ' + tryCount);
            }
         //   worker.jSBalanceChange(balance);
        } catch (ex) {
           //console.log('Balance not parse: ' + ex);
          // worker.JSBalanceChange(-2);
        }
    } else {
        console.log('Нет авторизации!');
     //  worker.JSBalanceChange(-2);
    }
}

function sendBalanceAndLogin() {
    worker.islogin = checkLogin();
    //console.log('Статус авторизации: ' + worker.Islogin);
    ////worker.JSLogined();
    updateBalance();
}

function notifyOfBalance() {
    //var balance = getBalance();
    //worker.JSBalanceChange(balance);
}

function logWrite(logSting) {
    window.console.info(logSting);
    //console.log(logSting);
}

function failLogin(failMessage) {
    logWrite(failMessage);
    worker.islogin = false;
    ////worker.JSLogined();
}

async function login() {
    if (worker.LoginTry > 3) {
        failLogin('Лимит попыток превышен. Похоже что вы указали неверно логин/пароль!');
        return;
    }
    let isLogin = await asyncCallbackLong(() => checkLogin(), 30, 20);

    if (isLogin) {
        removeLivetex();

        worker.Islogin = true;
        //worker.JSLogined();
        //notifyOfBalance();
        logWrite('Есть авторизация');
        return;
    }
    logWrite('Статус авторизации LigaStavok: ' + isLogin);
    var loginbtninview = await asyncCallbackHot(() => {
        return !!isInView(document.querySelector("button[class*=\'auth-panel__button_sign-in-\']"));//кнопка входа
    });
    if (!loginbtninview) {
        failLogin("Кнопка входа так и не загрузилась!");
        return;
    }
    var loginBtn = document.querySelector("button[class*=\'auth-panel__button_sign-in-\']");
    loginBtn.click();

    let isLoginWindowShowing = await asyncCallbackHot(() => {
        return !!isInView(window.document.querySelector('input[name="mobilePhone"]')) || !!isInView(window.document.querySelector('input[name="email"]'));//кнопки входа по номеру или по почте
    });
    if (!isLoginWindowShowing) {
        failLogin("Форма логина так и не загрузилась!");
        return;
    }

    var loginMethodTypes = document.querySelectorAll("[class*='tab-switcher__name-']");//выбор входа
    var login = worker.login;
    var type = "email";
    if (login.indexOf('@') === -1) {
        loginMethodTypes[0].click();
        type = "mobilePhone";
        if (login.length === 11) {
            login = login.substring(1);
        } else if (login.length !== 10) {
            failLogin("Неправильный телефон! Используйте email.");
            return;
        }
        login = "(" + login.substring(0, 3) + ")  " + login.substring(3, 6) + "-" + login.substring(6);
    }
    worker.loginTry = worker.loginTry + 1;
    let loginInput = document.querySelector('input[name="' + type + '"]');
    let passwordInput = document.querySelector('input[name="password"]');//окно пароля
    setValueWithEvent(loginInput, login);
    setValueWithEvent(passwordInput, worker.password);

    loginbtninview = await asyncCallbackHot(() => {
        return !!isInView(document.querySelector("[class*='auth-form__submit-']"));
    });

    if (!loginbtninview) {
        failLogin("Кнопка входа так и не загрузилась!");
        return;
    }
    await sleep(10000);
    document.querySelector("[class*=auth-form__submit-]").removeAttribute("disabled");
    document.querySelector("[class*=auth-form__submit-]").click();
    let isLogined = await asyncCallbackHot(() => checkLogin());

    if (!isLogined) {
        failLogin('Авторизация, так и не прошла');
        return;
    }
    sendBalanceAndLogin();
}

function checkLogin() {
    try {
        
        let element = window.document.querySelector("[class*='auth-panel_signed']");//блок счета и истории ставок, доступно после лог-ии
        return element !== undefined && element != null;//проверка на ошибки
    } catch (e) {
       //console.log("islogined check");
        return false;
    }
}

function reactInstance(element) {
    let s = Object.keys(element).find(function (k) {
        return k.indexOf("__reactInternalInstance") >= 0;
    });
    if (s === undefined) {
        return undefined;
    }
    return element[s];
}

function reactEventHandlers(element) {
    let s = Object.keys(element).find(function (k) {
        return ~k.indexOf("__reactEventHandlers")
    });
    if (s === undefined) {
        return undefined;
    }
    return element[s];
}

function getRightSidebarStateNode() {// состояние купона
    let betslip = document.querySelector("[class^='sidebar-right-']");//купон
    console.dir(betslip)
    if (!betslip) {
        console.log("getRightSidebarStateNode 3")
        return undefined;
    }
    console.log("getRightSidebarStateNode 4")
    let props = null;
    
    console.log("!!reactInstance(betslip)");
    console.log(!!reactInstance(betslip));
    console.log("!!reactInstance(betslip).return");
    console.log(!!reactInstance(betslip).return);
    console.log("!!reactInstance(betslip).return.stateNode");
    console.log(!!reactInstance(betslip).return.stateNode);
    try{
        props = reactInstance(betslip).return.stateNode;
    }catch (e) {
        console.log("произошла ошибка на 534")
    }
    console.log("props")
    console.log(!!props)
    console.log(!!reactInstance(betslip))
    console.log(!!reactInstance(betslip).return)
    
    console.log("getRightSidebarStateNode 5")
    if (!props) {
        return undefined;
    }
    console.log("getRightSidebarStateNode 6")
    return props;
}

function getTransport() {
    let stateNode = getRightSidebarStateNode();
    if (!stateNode) {
        return undefined;
    }
    return stateNode.context.transport;
}

function getBetslipProps() {//сведение для купона
    let stateNode = getRightSidebarStateNode();
    if (!stateNode) {
        return undefined;
    }
    return stateNode.props.betslip;
}

function getStakeCount() {
    console.log("stake count1 ")
    let props = getBetslipProps();
    console.log("stake count2")
    if (!props) {
        console.log("stake count3")
        return NaN;
    }
    console.log("stake count4")

    if (!props.bets) {
        console.log("stake count5")
        return 0;
    }
    console.log("stake count6")
    return props.bets.length;
}

function getMinStake() {
    let props = getBetslipProps();
    if (!props) {
        return NaN;
    }

    if (!props.bets || props.bets.length !== 1) {
        return NaN
    }

    let number = parseFloat(props.bets[0].minAmount);
    if (isNaN(number)) {
        let elementNodeListOf = document.querySelectorAll('[class^=betslip__limit-value]');
        if (elementNodeListOf && elementNodeListOf.length === 2 && elementNodeListOf[0]) {
            let s = elementNodeListOf[0].textContent;
            if (s.length > 0) {
                return parseFloat(s);
            }
        }
    }
    return number;
}

function getMaxStake() {
    let props = getBetslipProps();
    if (!props) {
        
        return NaN;
    }
 
    if (!props.bets || props.bets.length !== 1) {
        
        return NaN
    }
    let number = parseFloat(props.bets[0].maxAmount);
    if (isNaN(number)) {
        let elementNodeListOf = document.querySelectorAll('[class^=betslip__limit-value]');
        if (elementNodeListOf && elementNodeListOf.length === 2 && elementNodeListOf[1]) {
            let s = elementNodeListOf[1].textContent.replace(" ", "");
            if (s.length > 0) {
                return parseFloat(s);
            }
        }
    }
    return number;
}

function getSummFromStake() {
    let props = getBetslipProps();
    if (!props) {
        return NaN;
    }
    if (!props.bets || props.bets.length !== 1) {
       //console.log("Не можем определить сумму в купоне");
        return NaN;
    }
    return parseFloat(props.bets[0].amount);
}

function clearCoupon() {//кнопка отчислить корзину
    let btn = document.getElementById("betslip__button_clear");
    if (btn) {
        btn.click();
    }
}

function getCoefFromCupon() {
    let props = getBetslipProps();
    if (!props) {
        return NaN;
    }
    if (!props.bets || props.bets.length !== 1) {
        //console.log("Открыто купонов больше 1");
        return NaN;
    }
    return parseFloat(props.bets[0].odd);
}

function getParametrFromCupon() {
    let props = getBetslipProps();
    if (!props) {
        return NaN;
    }
    if (!props.bets || props.bets.length !== 1) {
        //console.log("Открыто купонов больше 1");
        return NaN;
    }
    let bet = props.bets[0];
    let base = parseFloat(bet.base);
    if (base === 0) {
        if (bet.marketType === "HAN") {
            return parseFloat(0);
        }
        return -6666;
    }
    return parseFloat(base);
}

function checkStakeEnabled() {
    if (!candostake) {
        return false;
    }
    let props = getBetslipProps();
    if (!props) {
        return false;
    }
    if (!props.bets || props.bets.length !== 1) {
       //console.log("Открыто купонов больше 1");
        return false;
    }
    let bet = props.bets[0];
    return !bet.disabled && !bet.locked;
}

function getStakeGroup() {
    let props = getBetslipProps();
    if (!props) {
        return false;
    }
    if (!props.bets || props.bets.length !== 1) {
      //console.log("Открыто купонов больше 1");
        return false;
    }
    let bet = props.bets[0];
    return bet.group;
}

function getBalance() {
    let auth_panel = document.querySelector("[class^='auth-panel-']");
    if (!auth_panel) {
       //console.log("нет панели с авторизацией");
        return -1;
    }
    var wallet = reactInstance(auth_panel).return.stateNode.props.currentWallet;//показывает валюту и кол-во денег
    if (!wallet) {
        return -1
    }
    return parseFloat(wallet.balance);
}

function setValueWithEvent(e, t) {//установка событий
    let n = e;
    let r = n.value;
    n.value = t;
    let o = new Event("input", { bubbles: !0 });
    o.simulated = !0;
    let i = n._valueTracker;
    i && i.setValue(r) && n.dispatchEvent(o);
}

var lastStake = null;

function getstakeInfo() {
    //console.log(`getstakeInfo from ${worker.BookmakerName}`);
    if (!worker.stakeInfo) {
        console.log('stakeInfo==null. Error');
    }
    worker.stakeInfo.Auth = checkLogin();
    worker.stakeInfo.StakeCount = getStakeCount();

    worker.stakeInfo.Balance = getBalance();
    worker.stakeInfo.MinSumm = parseFloat(getMinStake());
    worker.stakeInfo.MaxSumm = parseFloat(getMaxStake());
    worker.stakeInfo.Summ = getSummFromStake();

    if (worker.stakeInfo.StakeCount === 1) {
        worker.stakeInfo.IsEnebled = checkStakeEnabled();
        worker.stakeInfo.coef = getCoefFromCupon();

        worker.stakeInfo.parametr = getParametrFromCupon();
    }

    console.dir(worker.stakeInfo);
    lastStake = worker.stakeInfo;
    return JSON.stringify(worker.stakeInfo);
}

function ligaSportId(sportId) {
    switch (sportId) {
        case 1: 
            return 33;
        case 2: 
            return 34;
        case 3:
            return 31;
        case 4:
            return 25;
        case 5:
            return 128;
        case 6:
            return 30;
        case 7:
            return 1246;
        case 8:
            return 135;
        case 13:
            return 1124;
        case 16:
            return 24;
        default:
            return -1;
    }
}

async function selectCurrentSport() {
    let elem = document.querySelector("[class^=live__events]");//основное окно, все мотчи лайва
    if (!elem) {
        console.log("не получилось выбрать спорт");
        return false;
    }
    let ligaSport = ligaSportId(worker.SportId);
    if (ligaSport === -1) {
        console.log("неправильный спорт");
        return false;
    }
    let has_react_children = await asyncCallbackLong(() => {
        return reactEventHandlers(elem).children[1] !== undefined
    }, 30, 12);
    if (!has_react_children) {
        return true;
    }

    let react_children = reactEventHandlers(elem).children[1];
    for (var i = 0; i < react_children.length; i++) {
        if (parseInt(react_children[i].key) === ligaSport) {
            let it = react_children[i].props.children[1].props.children[1].props;
            let need = it.total - it.current;
            if (need > 0) {
                it.onClick(need, new Event(Event.NONE));
                await sleep(100);
            }
        }
    }
    return false;
}

async function selectEvent(rows, id) {

    for (var i = 0; i < rows.length; i++) {
        let rowInfo = rows[i];
        let reacted = reactInstance(rowInfo);
        let eventid = reacted.child.memoizedProps.eventId;
        if (parseInt(eventid) === parseInt(id)) {
            reactEventHandlers(rowInfo.querySelector("a")).onClick();
            await sleep(100);
            return await asyncCallbackLong(() => {
                return !!isInView(document.querySelector("[class^=event__wrapper-events-]"));
            }, 30, 20);
        }
    }
   //console.log('Событие не найдено!');
    return false;
}

async function couponOpened() {
    try {
        if (!checkBet()) {
           //console.log('Не прошла проверка ставок!');
          //worker.JSFail();
            return false;
        }
    } catch (e) {
        //console.log('Ошибка проверки ставок:' + e);
    }
    sendBalanceAndLogin();
    dostaketry = 0;
    worker.JSStop();
}

async function fastOpenLive() {
    let liveButton = document.querySelector("[class*=nav-button__primary-]");
    if (!liveButton) {
      //console.log('Не нашли кнопку LIVE');
        return false;
    }
    liveButton.click();
    return await asyncCallbackHot(() => {
        return !!isInView(document.querySelector("[class^=live__events-block-]")) && !isInView(document.querySelector("[class*=live__events-block_loading-]"));
    });
}

async function clickEventInLive(eventId) {
    if (document.location.pathname !== "/bets/live") {
       //console.log('Открываем LIVE');
        if (!await fastOpenLive()) {
          //console.log('Не смогли открыть LIVE');
            return false;
        }
    }
    await sleep(50);
    await selectCurrentSport();
    await sleep(200);
    let row_infos = document.querySelectorAll("[class^=bui-event-row__info-]");
    let selected = await asyncCallbackLong(() => {
        return !!selectEvent(row_infos, eventId);
    }, 100, 25);

    if (!selected) {
       //console.log('Не удалось выбрать событие');
        return false;
    }
    return true;
}

async function checkStakeOpened(betId, eventId) {
    let stakeCount = getStakeCount();
    if (stakeCount > 0) {
        if (stakeCount === 1) {
            
            let bet = getBetslipProps().bets[0];
            if (betId === bet.id && eventId === bet.eventId) {
              //console.log('Купон уже открыт.');
                return true;
            } else {
               //console.log('В купоне другие ставки, очищаем');
                clearCoupon();
            }
        } else {
            clearCoupon();
        }
    }
    return false;
}



let headers = {
    "Content-Type": "application/json",
    "Origin": "https://www.ligastavok.ru",
    "Referer": "https://www.ligastavok.ru/bets/live",
    "x-api-cred": " | ",
    "x-application-name": "desktop"
}

async function showStake() {
    clickCouponTab();
    candostake = true;
    if (!checkLogin()) {
        failLogin("нет авторизации");
       worker.JSFail();
        return;
    }
    if (dostaketry > maxstaketry) {
        throw "Превышено максимальное кол-во попыток проставки";
    }

    notifyOfBalance();
    sendBalanceAndLogin();

    var sel = window.unescape(worker.BetId).split('|');
    var betId = parseInt(sel[1]);
    var factorId = parseInt(sel[2]);
    var eventId = parseInt(worker.EventId);

    if (await checkStakeOpened(betId, eventId)) {
        await couponOpened();
        return;
    }

    removeLivetex();

    let json = await fetch("https://lds-api.ligastavok.ru/rest/events/v1/actionLines", {
        "method": "POST",
        "body": '{"ids": [' + String(worker.EventId) + ']}',
        "headers": headers
    }).then(f => f.json()).catch(r => r);

    let result = json.result;
    if (!result) {
        //console.log('Ошибка загрузки');
        
       //worker.JSFail();
        return;
    }
    if (result.length === 0) {
       //console.log('Не смогли найти событие');
       //worker.JSFail();
        return;
    }
    let res = result[0];
    if (!res.hasUnlocked) {
       //console.log('Прием ставок закрыт');
       //worker.JSFail();
        return;
    }
    let outcomes = res.outcomes;
    var bet = null;
    for (var i in outcomes) {
        if (bet) {
            break;
        }
        if (!outcomes.hasOwnProperty(i)) {
            continue
        }
        let rootNsub = outcomes[i].nsub;
        for (var j in rootNsub) {
            if (bet) {
                break;
            }
            if (!rootNsub.hasOwnProperty(j)) {
                continue;
            }
            let childNsub = rootNsub[j].nsub;
            for (var k in childNsub) {
                if (!childNsub.hasOwnProperty(k)) {
                    continue;
                }
                let b = childNsub[k];
                if (b.id === betId) {
                    bet = b;
                    break;
                }
            }
        }
    }
    if (!bet) {
      //console.log('Не смогли найти ставку');
       //worker.JSFail();
        return;
    }
    let bsprops = getBetslipProps();
    let newbet = {
        id: bet.id,
        odd: bet.value,
        factorId: bet.facId,
        type: bet.marketTitle,
        group: bet.partTitle,
        name: bet.title,
        gameId: bet.eventGameId,
        teams: bet.eventTeam,
        base: bet.adValue,
        eventId: bet.eventId,
        partName: bet.partName,
        marketId: bet.marketId,
        marketType: bet.marketType,
        outcomeKey: bet.outcomeKey,
        locked: bet.locked,
        comment: bet.eventComment,
        eventType: bet.eventType,
        topicId: bet.topicId
    };
    let rightPanelState = getRightSidebarStateNode();
    let transport = rightPanelState.context.transport;
    let ev = {bet: newbet, transport: transport, type: "@@betslip/ADD_BET"};
    rightPanelState.props.dispatch(ev, transport);

    let stakeopened = await asyncCallbackLong(() => {
        return getStakeCount() === 1
    }, 10, 40);
    if (!stakeopened) {
        //console.log('Купон не раскрылся!');
        //worker.JSFail();
        return;
    }
    let showedmax = await asyncCallbackLong(() => {
        return !isNaN(getMaxStake())
    }, 50, 40);
    if (!showedmax) {
       //console.log('не показывается max');
       //worker.JSFail();
        return
    }
    await couponOpened();

    
}


var checkingBetHistory = false;
var maxstakecheck = 5;
var currentstakecheck = 0;

var forceplaced = false;



function clickCouponTab() {
    reactEventHandlers(document.querySelectorAll(".sidebar-right__tab-771a6f")[0]).onClick()
}

function clickBetHistoryTab() {
    reactEventHandlers(document.querySelectorAll(".sidebar-right__tab-771a6f")[1]).onClick()
}

var laststakeid = 0;

function checkStakeStatus() {
    let doneMsg = document.querySelector("[data-type=done]");
    if (forceplaced || doneMsg) {
        if (doneMsg && !forceplaced) {
            let desc = document.querySelector("[class^=betslip__service-message-descr] a");
            if (!desc) {
                laststakeid = 0;
            } else {
                laststakeid = parseInt(desc.textContent.split(" ")[1])
            }
        }
        updateBalance();
       //console.log('Ставка принята: true');
        clearCoupon();
        return true;
    }
    let errorMsg = document.querySelector("[class^=betslip__service-message][data-type=danger-warning]");
    if (errorMsg) {
        if (errorMsg.textContent.indexOf("обновления коэффициента") > -1) {
            candostake = false;
        }
       //console.log('Ошибка принятия купона\n' + errorMsg.textContent);
        return false;
    }

    return false;
}

async function sleep(msec) {
    return  new Promise(resolve => setTimeout(resolve, msec));
}

async function asyncCallback(callbackBoolean) {
    var tryCounter = 0;
    while (tryCounter < 10) {
        if (callbackBoolean()) {
            return true;
        }
        tryCounter++;
        await sleep(1000);
    }
    return false;
}

async function asyncCallbackLong(callbackBoolean, sleepTime, tryCount) {
    var tryCounter = 0;
    while (tryCounter < tryCount) {
        if (await callbackBoolean()) {
            return true;
        }
        tryCounter++;
        await sleep(sleepTime);
    }
    return false;
}

async function asyncCallbackHot(callbackBoolean) {
    return await asyncCallbackLong(callbackBoolean, 100, 100);
}

function isInView(element) {
    if (!element) return false;
    return element.getBoundingClientRect().width !== 0;
}

function fireEvent(element, event) {
    var evt = document.createEvent('HTMLEvents');
    evt.initEvent(event, true, true);
    element.dispatchEvent(evt);
}

function nativeInputText(inputElement, text) {
    for (let index = 0; index < text.length; index++) {
        const element = text[index];
        if (inputElement !== window.document.activeElement) {
            inputElement.focus();
        }
        Api.DomEventshelper.KeyPress(element.charCodeAt());
    }
}

async function inputTextAsync(inputElement, text) {
    nativeInputText(inputElement, text);
    return await asyncCallbackHot(() => {
        return inputElement.value === text;
    });
}

function checkBetPeriod(bet) {
    let bn = worker.BetName.toLowerCase();
    let splited = bn.split(" (");
    if (splited.length === 1) {
        if (bet.partName === 'main' || bet.partName === 'ot') {
            return true;
        } else {
            
            
            
            
            window.console.log('Ошибка! Не тот период');
            return false;
        }
    }
    return true;
}

function checkIndTotals(bn, bet) {
    if (bn.indexOf("t1 ") > -1 || bn.indexOf("t2 ") > -1) {
        if (bet.marketType.indexOf('ITL') === -1) {
            return false;
        }
        if (bn.indexOf("under(") > -1) {
            return bet.name.toLowerCase() === 'мен';
        }
        if (bn.indexOf("over(") > -1) {
            return bet.name.toLowerCase() === 'бол';
        }
        
    }
    return true;
}

function checkTotals(bn, bet) {
    if (bn.indexOf("t1 ") > -1 || bn.indexOf("t2 ") > -1) {
        if (!checkIndTotals(bn, bet)) {
          //console.log('Не прошла проверка на индивидуальный тотал');
            return false;
        }
        return true;
    }
    if (bn.indexOf("under(") > -1) {
        return bet.marketType === 'TTL' && bet.name.toLowerCase() === 'мен';
    }
    if (bn.indexOf("over(") > -1) {
        return bet.marketType === 'TTL' && bet.name.toLowerCase() === 'бол';
    }
    return true;
}

function checkWin(bn, bet) {
    if (bn.indexOf("win 1") > -1 || bn === "1" > -1) {
        return bet.marketType === 'WIN' && bet.name.toLowerCase() === '1';
    }
    if (bn.indexOf("win 2") > -1 || bn === "2" > -1) {
        return bet.marketType === 'WIN' && bet.name.toLowerCase() === '2';
    }
    if (bn === "x") {
        return bet.marketType === 'WIN' && bet.name.toLowerCase() === 'x';
    }
    if (bn === "1x") {
        return bet.marketType === 'DBL' && bet.name.toLowerCase() === '1x';
    }
    if (bn === "x2") {
        return bet.marketType === 'DBL' && bet.name.toLowerCase() === 'x2';
    }
    if (bn === "12") {
        return bet.marketType === 'DBL' && bet.name.toLowerCase() === '12';
    }
    return true;
}

function isRestricted() {
    return parseFloat(worker.stakeInfo.MaxSumm) === 555.0 || parseFloat(worker.stakeInfo.MaxSumm) === 300.0;
}

function checkBet() {
    let bet = getBetslipProps().bets[0];
    if (!checkBetPeriod(bet)) {
       //console.log('Не прошла проверка на период ставки');
        return false;
    }

    let bn = worker.BetName.toLowerCase();
    
    
    
    
    if (!checkWin(bn, bet)) {
       //console.log('Не прошла проверка на выигрыш');
        return false;
    }

    if (worker.CheckRestriction && isRestricted()) {
        var paused = worker.SetToPause();
       worker.helper.SendInformedMessage('Аккаунт ligastavok порезан. ' + (paused ? "Поставлен на паузу" : "На паузу поставить не удалось"));
        return false
    }
    return true;
}
