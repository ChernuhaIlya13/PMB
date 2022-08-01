'use strict';
(async function()
{
    await CefSharp.BindObjectAsync('worker');

    document.addEventListener('DOMContentLoaded', async function (ev) {
        await worker.setFocus();
        let url = ev.path[0].domain;
        let badProxy = location.href === "chrome-error://chromewebdata/";
        if(badProxy){
            console.log("Плохой прокси");
            await worker.notificateInBrowserHtml("Change your proxy");
            return;
        }
        if(!!document.querySelector("#RemindMeLater")){
            document.querySelector("#RemindMeLater").click()
        }

        if(!!document.getElementsByClassName("pm-MessageOverlayCloseButton")[0]){
            document.getElementsByClassName("pm-MessageOverlayCloseButton")[0].click();
        }
        let pageLoaded = await asyncCallbackLong(()=>{
            return !!document.querySelector("body > div:nth-child(1) > div > div.wc-WebConsoleModule_SiteContainer > div.wc-WebConsoleModule_Header > div > div.hm-MainHeaderWide > div.hm-MainHeaderRHSLoggedOutWide > div.hm-MainHeaderRHSLoggedOutWide_LoginContainer > div")
                || !!document.getElementsByClassName("hm-MainHeaderMembersWide_Balance")[0];
        },1000,10);
        await sleep(500);
        if(pageLoaded){
            worker.domLoaded(url)
            var printSource = document.createElement("div");
            printSource.style.position = "absolute"
            printSource.style.top = "80%"
            printSource.style.left = "10%"
            printSource.style.borderRadius = "25px";
            printSource.style.textAlign = "center"
            printSource.style.fontSize = "20px"
            printSource.style.backgroundColor = "#838996"
            printSource.textContent = "Узнать исходный код страницы"
            printSource.addEventListener("click",async (e)=>{
                sourceCode = await worker.printSource();
            })
            document.body.append(printSource)
        }
    });
})();
let currentStake = null;
let sumOfStake = null;
let doStakeFirstTime = true;
let stemData = null;
let actualBalance = null;
let sourceCode = null;

async function easyLogin(login,pass){
    let isStartElementLoad = await asyncCallbackLong(()=>{
        return (isInView(window.document.querySelector('div.hm-HeaderModule'))
            && !document.querySelector("#__-plContainer > div.bl-Preloader_SpinnerContainer > div"))
            || window.document.getElementById('dv1')
    },1000,10);

    if (!isStartElementLoad) {
        console.log('Стартовая страница так и не загрузилась');
        return false;
    }

    if (isInView(window.document.getElementById('dv1'))) {
        console.log('Стартовая страница.');
        window.document.getElementById('dv1').children[0].click();
        return false;
    }

    let isLoadLine = asyncCallbackHot(() => {
        return isInView(window.document.querySelector('div.gll-Participant.li-InPlayParticipant'));
    }, "isLoadLine");

    if (!isLoadLine) {
        console.log('линия так и не загрузилась!');
        return false;
    }

    let loginBtnShowed = await asyncCallbackHot(() => {
        return !!isInView(window.document.querySelector('div.hm-MainHeaderRHSLoggedOutWide_Login'));
    });
    if(!loginBtnShowed){
        console.log('Не прогрузились кнопка вход');
        return false;
    }
    let loginBtn = window.document.querySelector('div.hm-MainHeaderRHSLoggedOutWide_Login');
    await worker.sendMouseWheel(Math.round(window.outerWidth / 2),Math.round(window.outerHeight / 2),Math.round(loginBtn.getBoundingClientRect().x),Math.round(loginBtn.getBoundingClientRect().y));
    loginBtn.wrapper.clickHandler();

    let loginContainer = await asyncCallbackLong(() => {
        return isInView(window.document.querySelector('div.lms-StandardLogin_Container'));
    }, 1000, 10);

    if (!loginContainer) {
        console.log('Не прогрузился контейнер логина');
        return false;
    }
    
    let inputLoginLoaded = await asyncCallbackLong(()=>{
        return isInView(document.querySelector('input.lms-StandardLogin_Username'))
    },1000,10);
    if(!inputLoginLoaded){
        console.log('Не прогрузился логин');
        return false;
    }
    let inputLogin = window.document.querySelector('input.lms-StandardLogin_Username');
    if (!inputLogin) {
        console.log('Нет поля логина');
        return false;
    }

    let inputPasswordLoaded = await asyncCallbackLong(()=>{
        return isInView(document.querySelector('input.lms-StandardLogin_Password'))
    },1000,10);
    if(!inputPasswordLoaded){
        console.log('Не прогрузился пароль');
        return false;
    }
    let inputPassword = document.querySelector('input.lms-StandardLogin_Password');
    if (!inputPassword) {
        console.log('Нет поля пароля');
        return false;
    }

    let loginButton = window.document.getElementsByClassName("lms-LoginButton")[0];
    if (!loginButton) {
        window.console.log('Нет кнопки входа');
        return false;
    }
    await worker.sendMouseWheel(Math.round(loginBtn.getBoundingClientRect().x),Math.round(loginBtn.getBoundingClientRect().y),Math.round(inputLogin.getBoundingClientRect().x),Math.round(inputLogin.getBoundingClientRect().y));

    simulate(inputLogin,login);
    await sleep(1000);
    await worker.sendMouseWheel(Math.round(inputLogin.getBoundingClientRect().x),Math.round(inputLogin.getBoundingClientRect().y),Math.round(inputPassword.getBoundingClientRect().x),Math.round(inputPassword.getBoundingClientRect().y));
    simulate(inputPassword,pass);
    await sleep(1000);
    await worker.sendMouseWheel(Math.round(inputPassword.getBoundingClientRect().x),Math.round(inputPassword.getBoundingClientRect().y),Math.round(loginButton.getBoundingClientRect().x),Math.round(loginButton.getBoundingClientRect().y));
    fireMouseEvent(loginButton, "mouseover");
    fireMouseEvent(loginButton, "mousemove");
    fireMouseEvent(loginButton, "click");
    loginButton.click();
    return true;
}
var trashStrings = [" (w)", " (y)", " (u21)", " (u20)", " (u19)"]

function removeTrashInNames(n) {
    var res = n;
    for (let i = 0; i < trashStrings.length; i++) {
        res = res.replace(trashStrings[i], "")
    }
    return res;
}

function removeGermanSym(n) {
    return n.replaceAll("ä", "a").replaceAll("ö", "o").replaceAll("ü", "u");
}

function isSimilarEventTeams(evn, t1, t2) {
    let evname = evn.toLowerCase();
    let t1l = removeTrashInNames(t1.toLowerCase())
    let t2l = removeTrashInNames(t2.toLowerCase())
    let evnameNonGerman = removeGermanSym(evname)

    return (evname.includes(t1l) && evname.includes(t2l)) ||
        (evnameNonGerman.includes(t1l) && evnameNonGerman.includes(t2l))
}

async function openNonFastEvent(eventid,firstTeam,secondTeam) {
    var navid = "";
    firstTeam = firstTeam.trim();
    secondTeam = secondTeam.trim();

    if (eventid.startsWith("6V155")) {
        navid = eventid.split("_")[0].substring(2)
    } else {
        let simpleEventId = (eventid.startsWith("6V") ? eventid.substring(2) : eventid)

        await asyncCallbackLong(()=>
                !!document.querySelectorAll("div.ovm-Fixture")
            ,1000,10);
        await sleep(5000);
        let events = document.querySelectorAll("div.ovm-Fixture");

        let existsCurrentEventId = Array.from(events).map(e => e.wrapper.stem?.data.ID.toString().split("_")[0]).includes(simpleEventId);
        if(!existsCurrentEventId){
            console.log("Не нашёл ивент")
            return false;
        }

        let event = Array.from(events).filter(e => {
            return e.wrapper.stem?.data.ID.toString().split("_")[0] === simpleEventId
        })[0];

        if(!event){
            console.log("Не нашёл ивент")
            return false;
        }

        let currentStem = event.wrapper.stem;

        if (currentStem) {
            navid = "EV" + currentStem.getLegacyInPlayNavigationID();
        }
    }
    if (!navid) {
        return false;
    }
    let navigaionid = "#IP#" + navid;
    window.ns_navlib_util.WebsiteNavigationManager.NavigateTo(navigaionid, {data: {needsCard: !1}}, true);
    let loadedEvent = await asyncCallbackLong(() => {
        let viewDetail = document.querySelector(".ipe-EventViewDetail");
        let hasStem = viewDetail && viewDetail.wrapper && viewDetail.wrapper.stem;//isSimilarEventTeams(viewDetail.wrapper.stem.data.NA, firstTeam, secondTeam)
        return location.href.includes(navid) && hasStem;
    }, 50,100);
    if (!loadedEvent) {
        console.log("Событие не открылось");
        return false;
    }
    let viewDetail = document.querySelector(".ipe-EventViewDetail");
    return !!viewDetail;
}

function clickteam(team) {
    let fixtureselected = team.wrapper.stem._eRegister.show;//использовал show,forceVisibilityUpdate,update,hide,delete
    if (null == fixtureselected || 0x0 === fixtureselected.length) return null;
    for (var names = Object.getOwnPropertyNames(fixtureselected), i = 0x0; i < names.length; i++){
        if (fixtureselected[names[i]] && fixtureselected[names[i]].method && fixtureselected[names[i]].scope){
            return {
                'method': fixtureselected[names[i]].method,
                'scope': fixtureselected[names[i]].scope
            }
        }
    };
    return null;
}

async function openEvent(eventId,firstTeam,secondTeam,sportId){
    let pressedInPlayButton = window.location.href.includes("IP/B1");

    if(!pressedInPlayButton){
        let inPlayBtn = document.querySelector("body > div:nth-child(1) > div > div.wc-WebConsoleModule_SiteContainer > div.wc-WebConsoleModule_Header > div > div.hm-MainHeaderWide > div.hm-MainHeaderCentreWide > div:nth-child(2)");
        await worker.sendMouseWheel(0,0,Math.round(inPlayBtn.getBoundingClientRect().x),Math.round(inPlayBtn.getBoundingClientRect().y));
        inPlayBtn.click();
    }
    let inPlayBtnLoaded = await asyncCallbackLong(() => {
        return !!document.querySelector("body > div:nth-child(1) > div > div.wc-WebConsoleModule_SiteContainer > div.wc-WebConsoleModule_Header > div > div.hm-MainHeaderWide > div.hm-MainHeaderCentreWide > div:nth-child(2)");
    },1000,10);

    if(!inPlayBtnLoaded){
        console.log("Нет кнопки для открытия всей линии")
        return false;
    }

    let iconBarLoaded = await asyncCallbackLong(() => {
        return !!window.document.querySelector('div.ovm-ClassificationBar_Contents ') || window.document.querySelector('div.ipo-ClassificationBar_ButtonContainer');
    },1000,10);

    if (!iconBarLoaded) {
        console.log('Нет панели со спортом');
        return false;
    }
    let iconBar = window.document.querySelector('div.ovm-ClassificationBar_Contents ') || window.document.querySelector('div.ipo-ClassificationBar_ButtonContainer');

    let betSportId = convertToSportId(Number(sportId));
    let sport = iconBar.querySelector('div.ovm-ClassificationBarButton-' + betSportId) || iconBar.querySelector('div.ipc-InPlayClassificationIcon-' + betSportId);

    if (!sport) {
        console.log('Спорт не найден!');
        return false;
    }
    await worker.sendMouseWheel(0,0,Math.round(sport.getBoundingClientRect().x),Math.round(sport.getBoundingClientRect().y));

    sport.click();

    await sleep(1000);
    var evid = eventId.split("_")[0];

    return await openNonFastEvent(evid, firstTeam, secondTeam);
}

async function openMarkets(){
    let marketGroups = document.querySelectorAll(".sip-MarketGroup");
    if(!marketGroups){
        console.log("Не смог найти маркеты для открытия,скорее всего были изменения на странице")
    }
    var anyopened = false;
    for (var i = 0; i < marketGroups.length; i++) {
        try {
            let wrapper = marketGroups[i].wrapper;
            if(!wrapper){
                console.log("Не нашёл wrapper на рынке,возможны изменения на странице")
            }
            if (!wrapper._open) {
                anyopened = true;
                wrapper.setOpen(true);
            }
        } catch (e) {
        }
    }
    if (anyopened) {
        await sleep(1000);
    }
}

async function findStakeAndClick(betId) {
    doStakeFirstTime = true;
    stemData = null;
    actualBalance = +window.Locator.user._balance.totalBalance;
    betId = betId.split("|")[0];//397634388|5/6|112432416|166.5
    await openMarkets();

    let betsNodes = getAllStakesNodes();
    if(betsNodes.length === 0){
        console.log("Количество ставок на странице равно нулю,возможны изменения на странице")
    }

    toggleMousemode();
    let idsStakes = betsNodes.map(e => e?.wrapper?.stem?.data?.ID);
    if(idsStakes.length === 0){
        console.log("Возможны изменения на странице")
    }
    let idExists = idsStakes.includes(betId);
    if(!idExists){
        console.log("Нет Id с такой ставкой")
        return false;
    }
    for (var i = 0; i < betsNodes.length; i++) {
        let element = betsNodes[i].wrapper;
        if (element && element.stem && element.stem.data && parseInt(element.stem.data.ID) === parseInt(betId)) {
            currentStake = element;
            stemData = currentStake.stem.data
            let blockOfStakes = document.querySelector(".ipe-EventViewDetailScroller")
            if(!blockOfStakes){
                console.log("Возможны изменения")
            }
            let nodeCoord = betsNodes[i].getBoundingClientRect()
            blockOfStakes.scrollTo(0,nodeCoord.y / 2)
            await sleep(300);
            betsNodes[i].style.border = '1px solid red';

            let isLoaded = await asyncCallbackLong(() => {
                return isInView(element._element);
            }, 50, 40);

            if (!isLoaded) {
                console.log("Возможны изменения");
                return false;
            }

            let isElementSuspend = await asyncCallbackLong(() => {
                return element._suspended !== true;
            }, 50, 10);

            if (!isElementSuspend) {
                console.log('Ставка была приостановлена');
                return false;
            }
            await clearCoupon();
            if(!window.Locator.user._balance.refreshBalance)
                console.log("Возможны изменения")
            window.Locator.user._balance.refreshBalance();
            return true;
        }
    }
    return false;
}

function setValueSum(summa){
    sumOfStake = summa;
    return true;
}

function addBetToCart(id, fi, od, zw) {
    const constructString = `pt=N#o=${od}#f=${fi}#fp=${id}#so=#c=1#mt=1#id=${zw}Y#|TP=BS${zw}#`;
    const uid = "";
    const pom = "1";
    const partType = "N";
    const getSportType = function () {
        return partType;
    };
    const getCastCode = function () {
        return "";
    };
    const key = function () {
        return zw;
    };
    const bet = {
        constructString,
        uid
    };
    window.BetSlipLocator.betSlipManager.addBet({
        betsource: "11",
        item: bet,
        action: 0,
        partType: partType,
        constructString: constructString,
        quickBetApplicable: true,
        decimalPlaces: 2,
        isBetBoosted: true,
        fixtureID: fi.toString(),
        participantID: id.toString(),
        subscribe: true,
        key: key,
        getSportType: getSportType,
        getCastCode: getCastCode,
        uid: `${fi}-${id}`
    });
    return window.BetSlipLocator.betSlipManager.betslip.getBetCount() === 1;
}
async function moveNearElement(nodeElement){
    let rect = nodeElement.getBoundingClientRect()
    let randomX = Math.random() * 10;
    let randomY = Math.random() * 10;
    await worker.sendMouseWheel(Math.round(rect.x), Math.round(rect.y) ,Math.round(rect.x + (rect.width / 2) + Math.round(randomX)),Math.round(rect.y + (rect.height / 2) + Math.round(randomY)));
}
function enabledCoupon() {
    let firstCoupon = document.querySelector(".bss-NormalBetItem, .qbs-NormalBetItem, .lbs-NormalBetItem, .lqb-NormalBetItem")
    if (!firstCoupon) {
        return false;
    }
    return !!isInView(firstCoupon) && !firstCoupon.classList.contains("bss-NormalBetItem_Suspended") && !firstCoupon.classList.contains("lqb-NormalBetItem_Suspended") && !firstCoupon.classList.contains("qbs-NormalBetItem_Suspended") && !firstCoupon.classList.contains("lbs-NormalBetItem_Suspended");
}

async function waitLoadedStake() {
    if (!betslipValid()) return false;
    if (!enabledCoupon()) {
        return false;
    }
    return await asyncCallbackLong(async () => {
            const bet = await currentBet();
            if (bet && bet._active_element) {
                if (showedAcceptBtn() && enabledCoupon()) {
                    acceptChanges();
                    return false;
                }
                return true;
            }
            return false;
        },
        10,
        100);
}
async function afterOpeningCouponInternal() {

    await asyncCallbackLong(() => {
        return betslipValid();
    }, 10, 100);

    let valid = await asyncCallbackLong(function () {
        return betslipValid();
    }, 11, 1000);
    if (!valid && getStakeCount() === 1) {
        return;
    }
    await waitLoadedStake();

    await asyncCallbackLong(async () => {
        return await getStakeCount() > 0;
    }, 10, 500);

    let bsm = activeSlipModule().slip;
    await moveFromNearTo(bsm._element);
}

async function doStake() {
    let resultDoStake = false;
    try{
        if(doStakeFirstTime){
            removeGeneralError();
            removePlaceBetError();
            await worker.sendMouseWheel(0,0,Math.round(currentStake._active_element.getBoundingClientRect().x),Math.round(currentStake._active_element.getBoundingClientRect().y));
            var stakeData = currentStake._active_element.wrapper?.stem?.data
            if(!stakeData && getStakeCount() === 0){
                console.log("Нет данных ставки для добавления в купон")
                doStakeFirstTime = true;
                return false;
            }
            await moveNearElement(currentStake._active_element);
            await moveFromNearTo(currentStake._active_element);
            await moveFromNearAndClickOn(currentStake._active_element);
            addBetToCart(stakeData.ID,stakeData.FI,stakeData.OD,stakeData.ZW)
            await afterOpeningCouponInternal()

            let rectStake = currentStake._active_element.getBoundingClientRect();
            let randomX = Math.random() * 30;
            let randomY = Math.random() * 30;
            await worker.sendMouseClick(Math.round(rectStake.x) + Math.round(randomX),Math.round(rectStake.y) + Math.round(randomY));
            //currentStake?.clickHandler();
            let couponLoaded =  await asyncCallbackLong(()=>{
                return document.querySelector("div.bss-StandardBetslip,div.lqb-NormalBetItem")?.getBoundingClientRect().height !== 0 
                    && document.querySelector("div.bss-StandardBetslip,div.lqb-NormalBetItem")?.getBoundingClientRect().height !== undefined;
            },1000,5);

            if(!couponLoaded){
                doStakeFirstTime = true;
                console.log("Возможны изменения")
                console.log("Купон не загрузился")
                return false;
            }
            let setStakeResult = await setStakeSum(sumOfStake);
            if(!setStakeResult){
                console.log("Возможны изменения")
                doStakeFirstTime = true;
                return false;
            }
            let slip = await asyncCallbackLong(()=>{
                return !!betSlipManager()?.betslip?.activeModule?.slip;
            },1000,10);
            if(!slip){
                console.log("Возможны изменения")
                doStakeFirstTime = false;
                return false;
            }
            resultDoStake = await doingStake();
        }else {
            resultDoStake = await doingStake();
        }
    }catch (e) {
        doStakeFirstTime = true;
        return false;
    }
    return resultDoStake;
}

function acceptChanges() {
    activeSlipModule()?.slip?.model?.acceptChanges();
}

function betslipValid() {
    let activeModule = activeSlipModule();
    return !activeModule.isCollapsed() &&
        getStakeCount() === 1 && (!!activeModule.slip.bet || !!activeModule.slip.bets && activeModule.slip.bets.length > 0);
}

function showedAcceptBtn() {
    if (!betslipValid()) {
        return false;
    }
    let slip = activeSlipModule().slip;
    if (!slip) {
        return false;
    }
    if (!!slip.bet) {
        let placement = slip.bet.betPlacement;
        return isInView(placement.acceptOnlyButton._element) || isInView(placement.acceptButton._element);
    }
    if (slip.footer.betWrapper) {
        return isInView(slip.footer.betWrapper.acceptButton._element);
    }
    return isInView(slip.footer.acceptButton._element);
}

async function doingStake(){
    
    let placeBetButtonLoaded = await asyncCallbackLong(()=>{
        return !!document.querySelector("div.bsf-PlaceBetButton,div.lqb-AcceptButton,div.lqb-BetPlacement,div.bsf-AcceptButton")
    },300,15);
    if(!placeBetButtonLoaded){
        console.log("Возможны изменения")
        return false;
    }
    //document.querySelector("div.bsf-PlaceBetButton").click()//первый варинт(1)

    if (showedAcceptBtn()) {
        acceptChanges();
    }
    let acceptButton = document.querySelector("div.bsf-PlaceBetButton,div.lqb-AcceptButton,div.lqb-BetPlacement,div.bsf-AcceptButton")
    if(!isInView(acceptButton)){
        console.log("Возможны изменения")
    }
    //betSlipManager()?.betslip?.activeModule?.slip?.bet?.betPlacement?.acceptButton?.clickHandler()//второй вариант(2)
    //window.BetSlipLocator.betSlipManager.betslip?.activeModule?.slip?.footer?.betWrapper?.acceptButton?.clickHandler()
    //window.betSlipManager().betslip.activeModule.slip.footer.betWrapper.acceptButton.clickHandler()
    let acceptButtonRect = acceptButton.getBoundingClientRect()
    let randomX = Math.random() * 30
    let randomY = Math.random() * 30
    await worker.sendMouseWheel(0,0,Math.round(acceptButtonRect.x),Math.round(acceptButtonRect.y));
    await worker.sendMouseClick(Math.round(acceptButtonRect.x) + Math.round(randomX),Math.round(acceptButtonRect.y) + Math.round(randomY));
    let isSameStake = currentStakeIsSame();
    if(!isSameStake){
        console.log("Ставка поменялась динамически,ставить не буду")
        return false;
    }
    if(!window.betSlipManager().betslip.activeModule.slip.footer.betWrapper.placeBetButton.clickHandler){
        console.log("Возможны изменения")
    }
    window.betSlipManager().betslip.activeModule.slip.footer.betWrapper.placeBetButton.clickHandler()

    window.Locator.user._balance.refreshBalance();
    let stakeInProgress = await asyncCallbackLong(()=>{
        return +window.Locator.user._balance.totalBalance !== actualBalance || hasInProgress();
    },30,100);
    if(stakeInProgress){
        doStakeFirstTime = true;
        return true;
    }
    return false;
}
function currentStakeIsSame(){
    let currentStemData = currentStake?.stem?.data
    for(let key in currentStemData){
        if(currentStemData[key] !== stemData[key]){
            return false;
        }
    }
    return true;
}

function getCoefCurrentStake(){
    let coefstring = currentStake._oddsText._text;
    return Number(coefstring);
}
function checkGameInTennis(){
    let gamesForException = [11,13,23,36];
    let markets = Array.from(document.querySelectorAll("div.sip-MarketGroup"))
    for(let i = 0;i < markets.length;++i){
        if(gamesForException.some(game => markets[i]?.firstElementChild.textContent.toLowerCase().includes(game))){
            return false;
        }
    }
    return true;
}
function nearToElement(elem, pixels = 35) {
    if (!isInView(elem)) {
        return false;
    }
    let rect = elem.getBoundingClientRect();
    let centerX = rect.x + rect.width / 2;
    let centerY = rect.y + rect.height / 2;
    if (centerX + pixels > window.innerWidth) {
        if (centerY + pixels > window.innerHeight) {
            return {
                x: centerX - pixels,
                y: centerY - pixels
            }
        } else {
            return {
                x: centerX - pixels,
                y: centerY + pixels
            }
        }
    } else {
        if (centerY + pixels > window.innerHeight) {
            return {
                x: centerX + pixels,
                y: centerY - pixels
            }
        } else {
            return {
                x: centerX + pixels,
                y: centerY + pixels
            }
        }
    }
}
async function moveFromNearTo(elem) {
    let near = nearToElement(elem);
    if (!near) {
        return;
    }
    let rect = elem.getBoundingClientRect();
    let randx = 10 * Math.random();
    let randy = 10 * Math.random();
    let startX = near.x,
        startY = near.y;
    let clickX = rect.x + rect.width / 2 + randx;
    let clickY = rect.y + rect.height / 2 + randy;
    await worker.sendMouseWheel(Math.round(startX), Math.round(startY),Math.round(clickX) ,Math.round(clickY) );
}
async function moveFromNearAndClickOn(elem) {
    let near = nearToElement(elem);
    if (!near) {
        return;
    }
    return await moveAndClick(elem, near.x, near.y);
}
async function moveAndClick(elem, startx = -1, starty = -1) {
    if (!isInView(elem)) {
        return false
    }
    if (!await scrollToElement(elem)) {
        return false;
    }
    let rect = elem.getBoundingClientRect();
    let randx = 10 * Math.random();
    let randy = 10 * Math.random();
    let startX = startx > -1 ? startx : window.innerWidth / 2 + randx,
        startY = starty > -1 ? starty : window.innerHeight / 2 + randy;
    let clickX = rect.x + rect.width / 2 + randx;
    let clickY = rect.y + rect.height / 2 + randy;
    await worker.sendMouseWheel(Math.round(startX),Math.round(startY) ,Math.round(clickX) ,Math.round(clickY));
    await worker.sendMouseClick(Math.round(clickX) ,Math.round(clickY) );
    return true;
}
async function scrollToElement(elem) {
    if (!isInView(elem)) {
        return false
    }
    let rect = elem.getBoundingClientRect();
    let scrollW = -60;
    let startX = rect.x + rect.width / 2;
    let startY = window.innerHeight / 2;
    let maxIters = 3000;
    let iter = 0;
    while ((rect.top - rect.height / 2) < 0 || (rect.bottom + rect.height / 2) > window.innerHeight) {
        let dy = scrollW * Math.sign((rect.top - rect.height / 2));
        await worker.sendMouseWheel(Math.round(startX),Math.round(startY), 0, Math.round(dy));
        await sleep(Math.random() * 10);
        rect = elem.getBoundingClientRect();
        if (iter > maxIters) {
            return false;
        }
        if (iter % 7 === 0) {
            await sleep(200);
        }
        iter++;
    }
    return true;
}

function isLogined(){
    return window?.Locator?.user?.isLoggedIn 
        && window?.Locator.user?._balance?.totalBalance !== undefined
        && document.querySelector("div.bl-Preloader_Spinner") == null
}
function getCoefStake(index){
    let betsNodes = getAllStakesNodes();
    return betsNodes[index].textContent.replaceAll(/[A-Za-zА-Яа-я]/g,"").trim().replace(".",",");
}
function getIndexForSport(header,homeAway){
    header = header.toLowerCase()
    if(header.includes("cricket")){
        return homeAway ? 0 : 1;
    }
    else if(header.includes("table tennis")) {
        return homeAway ? 0 : 1;
    }
    else if(header.includes("soccer")){
        return homeAway ? 0 : 2;
    }
    else if(header.includes("bowls")){
        return homeAway ? 0 : 2;
    }
    else if(header.includes("tennis")){
        return homeAway ? 0 : 2;
    }
    else if(header.includes("basketball")){
        return homeAway ? 2 : 5;
    }
    else if(header.includes("esports")) {
        return homeAway ? 0 : 1;
    }
    else if(header.includes("ice hockey")){
        return homeAway ? 2 : 5;
    }
    else if(header.includes("volleyball")){
        return homeAway ? 2 : 5;
    }
    else if(header.includes("darts")){
        return homeAway ? 2 : 5;
    }
    else if(header.includes("australian rules")){
        return homeAway ? 0 : 1;
    }else if(header.includes("snooker")){
        return homeAway ? 0 : 1;
    }
}
function getAllStakesNodes(){
    let ipoAllMarketsParticipantSelector = window.document.querySelectorAll('div.gll-Participant_General').length > 0 ? 'div.gll-Participant_General' : 'div.gl-Participant_General';
    let betsNodesFirstPart = window.document.querySelectorAll(ipoAllMarketsParticipantSelector);
    if(betsNodesFirstPart.length === 0){
        console.log("Возможны изменения")
    }
    let betsNodesSecondPart = window.document.querySelectorAll("div.gl-Market_General-cn2");
    if(betsNodesSecondPart.length === 0){
        console.log("Возможны изменения")
    }
    let betsNodes = [];
    betsNodesFirstPart?.forEach(function(node){
        betsNodes.push(node);
    });
    betsNodesSecondPart?.forEach(function(node){
        betsNodes.push(node);
    });
    return betsNodes
}

function getCoupleStakesCoefs(){
    let currentStakeActive = currentStake._active_element;
    let blockStakes = Array.from(currentStakeActive.parentElement.children)//все элементы блока,в котором была выбрана ставка
    let currentStakeIndexInContainer = 0;//под каким индексом находится ставка,которую мы нашли
    for(let i = 0;i < blockStakes.length;++i){
        if(blockStakes[i] === currentStakeActive){
            currentStakeIndexInContainer = i;
            break;
        }
    }
    
    let oppositeStakeInAnotherContainer = false;//ставка находится в другом блоке
    for(let i = 0;i < blockStakes.length;i++){
        if(blockStakes[i].classList.contains("gl-MarketColumnHeader")){
            oppositeStakeInAnotherContainer = true;
            break;
        }
    }
    //противоложная ставка
    let oppositeStakeNode = null
    /* 
       рынок => контейнеры => ставки
       Индекс контейнера в текущем рынке чтобы понять,где нам нужно искать противоложную ставку 
    */
    let blockIndexInMarketForCurrentStake = 0; 
    
    let market = currentStakeActive.parentElement.parentElement
    let countBlocksInMarket = market.children.length
    for(let i = 0;i < currentStakeActive.parentElement.parentElement.children.length;i++){
        if(currentStakeActive.parentElement.parentElement.children[i] === currentStakeActive.parentElement){
            blockIndexInMarketForCurrentStake = i;
            break;
        }
    }
    let currentStakeFirst = false;//в какой очередности возвращать коэффициенты ставок
    if(oppositeStakeInAnotherContainer){
        let parallelContainer = null;
        if(blockIndexInMarketForCurrentStake < countBlocksInMarket - 1){
            parallelContainer = currentStakeActive.parentElement.parentElement.children[countBlocksInMarket - 1]
            currentStakeFirst = true;
        }else {
            if(currentStakeActive.parentElement.parentElement.children[0].children[0].textContent.trim() !== ""){
                parallelContainer = currentStakeActive.parentElement.previousSibling.previousSibling
            }else{
                parallelContainer = currentStakeActive.parentElement.previousSibling
            }
        }
        oppositeStakeNode = parallelContainer.children[currentStakeIndexInContainer]
    }else {
        let parentElement = currentStakeActive.parentElement
        if(parentElement.children.length > 2){
            if(currentStakeIndexInContainer === 0){
                oppositeStakeNode = parentElement.children[2]
                currentStakeFirst = true
            }else{
                oppositeStakeNode = parentElement.children[0]
            }
        }else {
            if(currentStakeIndexInContainer === 0){
                currentStakeFirst = true
                oppositeStakeNode = parentElement.children[1]
            }else{
                oppositeStakeNode = parentElement.children[0]
            }
        }
    }
    let stakesResult = {
        FirstStakeCoef : 0,
        SecondStakeCoef : 0
    };
    let firstStakeCoef = currentStakeActive.children.length > 1 ?
        currentStakeActive
            .children[currentStakeActive.children.length - 1]
            .textContent
            .replaceAll(/[a-zA-ZА-Яа-я]/g,"")
            .trim()
            .replace(".",",")
        :
        currentStakeActive.textContent
            .replaceAll(/[a-zA-ZА-Яа-я]/g,"")
            .trim()
            .replace(".",",")
    
    let secondStakeCoef = oppositeStakeNode.children.length > 1 ?
        oppositeStakeNode
            .children[oppositeStakeNode
            .children.length - 1]
            .textContent
            .replaceAll(/[a-zA-ZА-Яа-я]/g,"")
            .trim()
            .replace(".",",")
        :
        oppositeStakeNode.textContent
            .replaceAll(/[a-zA-ZА-Яа-я]/g,"")
            .trim()
            .replace(".",",")
    if(currentStakeFirst){
        stakesResult.FirstStakeCoef = firstStakeCoef
        stakesResult.SecondStakeCoef = secondStakeCoef;
    }else{
        stakesResult.FirstStakeCoef = secondStakeCoef
        stakesResult.SecondStakeCoef = firstStakeCoef
    }
    return stakesResult
}

function getCoefWinStake(homeAway){
    // "Soccer","Tennis","Bowls",[0][2]
    // "Basketball",[2][5]
    // "Cricket","Table Tennis" [0],[1]
    // "Darts" [0] [3]
    // "Esports","Ice Hockey","Volleyball ","Australian Rules",[0] [5]
    let eventHeader = document.getElementsByClassName("ipe-EventHeader_BreadcrumbText")[0].textContent
    let sportIndex = getIndexForSport(eventHeader,homeAway);
    let betsNodes = getAllStakesNodes();
    
    return betsNodes[sportIndex]?.textContent.replaceAll(/[A-Za-z]/g,"").trim().replace(".",",");
}


async function clearCoupon() {
    let cnt = await getStakeCount();
    if (cnt === 0) {
        return;
    }
    await asyncCallbackHot(() => {
        let bm = betSlipManager();
        if(!bm){
            console.log("Возможны изменения")
        }
        return bm && bm.betslip && bm.betslip.activeModule && !!bm.betslip.activeModule.slip
    })
    removeGeneralError();
    removePlaceBetError();
    if(!betSlipManager().deleteAllBets)
        console.log("Возможны изменения")
    betSlipManager().deleteAllBets();
    if(!betSlipManager().betslip.deleteAllBets)
        console.log("Возможны изменения")
    betSlipManager().betslip.deleteAllBets();

    await asyncCallbackLong(() => {
            let bs = betSlipManager().betslip;
            if (bs.activeModule.slip.bet || bs.activeModule.slip.bets && (bs.activeModule.slip.bets.length > 0 ||
                bs.getBetCount() > 0)) {
                betSlipManager().deleteAllBets();
                bs.deleteAllBets();
                return false;
            }
            return true;
        },
        20,
        100);
}

function activeSlipModule() {
    let bsm = betSlipManager();
    return bsm && bsm.betslip && bsm.betslip.activeModule;
}

function removeGeneralError() {
    const error = betSlipManager().betslip.activeModule.slip.generalErrorMessage;
    if(!error)
        console.log("Возможны изменения")
    if (error && error.getElement() && isInView(error.getElement())) {
        betSlipManager().betslip.activeModule.slip.removeErrorMessage();
    }
}

async function currentSlip(wait = 100) {
    let slip = null;
    await asyncCallbackLong(() => {
            slip = activeSlipModule().slip;
            return !!slip;
        },
        10,
        wait);
    return slip;
}

async function getStakeBox() {
    const slip = await currentSlip(5);
    let sb = null;
    if (slip && !!slip.bet) {
        sb = slip.bet.stakeBox;
    } else if (slip && slip.footer) {
        let f = slip.footer;
        if (f.stakeBox) {
            sb = f.stakeBox;
        } else {
            sb = slip.bets[0].stakeBox;
        }
    }
    return sb;
}

function removePlaceBetError() {
    const placeMessage = betSlipManager().betslip.activeModule.slip.placeBetErrorMessage;
    if(!placeMessage)
        console.log("Возможны изменения")
    if (placeMessage && placeMessage._element && isInView(placeMessage._element)) {
        betSlipManager().betslip.activeModule.slip.placeBetErrorMessageGotoMybetsClicked();
    }
}

async function setStakeSum(sum){
    try{
        //let bet = await currentBet();
        // await sleep(1000);
        let stakeBox = await getStakeBox();
        if (!!stakeBox) {
            if (stakeBox.setStake) {
                stakeBox.setStake(sum.toString());
            }
            // bet.stakeBox.appendStakeInput(sum.toString());
            // bet.stakeBox.updateStake(sum.toString());
            //fireEvent(bet.stakeBox.stakeValueInputElement, 'input');

            let slipManager = betSlipManager();
            let slip = slipManager && slipManager.betslip && slipManager.betslip.activeModule.slip;
            if (slip && slip.betCreditsHeader && slip.betCreditsHeader.canShow && !slip.betCreditsModeActive) {
                slip.betCreditsHeader.clickHandler();
            }
            await sleep(700);

            //return bet?.stakeBox?.stakeEnteredValue != "0.0" && bet?.stakeBox?.stakeEnteredValue != "";
            return stakeBox.stakeEnteredValue !== "0.0" && stakeBox.stakeEnteredValue !== "";
        }
        return false;
        
    }catch (e) {
        return false;
    }
}

let dictionary = {
    "please note that the minimum unit stake is":"Минимальная сумма ставки изменилась",
    "the selection is no longer available":"Выбор текущей ставки больше не доступен",
    "the price of your selection has changed":"Ставка была заменена",
    "the line of your selection has changed":"Строка вашего выбора изменилась",
    "the price and line have changed":"Ставка и линия были изменены"
};

async function checkWindowResult(){
    try{
        //'Please note that the minimum unit stake is 1.50'//если ставим меньше,чем нужно,выскакивает этот текст
        //'Bet Placed'//ставка была поставлена идеальна
        //'The selection is no longer available'-если ставка была изменена
        //'The price of your selection has changed'-ставка была изменена

        let messageBodyCouponLoaded = await asyncCallbackLong(()=>{
            return !!document.querySelector("div.bss-ReceiptContent_Title,div.lqb-QuickBetHeader_MessageBody");
        },1000,10);

        if(!messageBodyCouponLoaded){
            console.log("Купона нет на странице");
            return false;
        }
        //Признак блокировки аккаунта полностью,невозможно даже поставить
        //Certain restrictions may be applied to your account. If you have an account balance you can request to withdraw these funds now by going to the Withdrawal page in Members.
        //document.querySelector("div.bs-DefaultMessage_MessageText")//В этом элементе
        // let puttedStake = await asyncCallbackLong(()=> {
        //     return hasReceipt();
        // },1000,10);
        // if(!puttedStake)
        //     return false;
        let messageBodyCoupon = document.querySelector("div.bss-ReceiptContent_Title,div.lqb-QuickBetHeader_MessageBody");
        // for(let key in dictionary){
        //     if(messageBodyCoupon.textContent.toLowerCase().includes(key)){
        //         return false;
        //     }
        // }
        //если баланс меняется и не меняется количество ставок,то ориентируемся на баланс
        //иначе не ориентируемся на баланс
        if
        (
            messageBodyCoupon.textContent.toLowerCase().includes("bet placed")
            && document.querySelector("div.bss-ReceiptContent_Done,div.lqb-QuickBetHeader_DoneButton").textContent.toLowerCase() === "done"
            && document.querySelector("div.bss-ReceiptContent_Done,div.lqb-QuickBetHeader_DoneButton").getBoundingClientRect().x !== 0
            && document.querySelector("div.bss-ReceiptContent_Done,div.lqb-QuickBetHeader_DoneButton").getBoundingClientRect().y !== 0
        )
        {
            if(!!document.querySelector("div.bss-ReceiptContent_Done,div.lqb-QuickBetHeader_DoneButton")){
                document.querySelector("div.bss-ReceiptContent_Done,div.lqb-QuickBetHeader_DoneButton").click()//кликает на кнопку Done в купоне,когда ставка успешна
            }
            await sleep(500);
            return true;
        }
    }catch{
    }
    return false;
}

async function currentBet(wait = 100) {
    let bet = null;
    await asyncCallbackLong(() => {
            let slip = betSlipManager().betslip.activeModule.slip;
            let bets = slip.bets
            bet = slip.bet || bets && bets[0];
            return !!bet;
        },
        10,
        wait);
    return bet;
}

function betSlipManager() {
    return window.BetSlipLocator.betSlipManager;
}

function convertToSportId(sportId) {
    switch (sportId) {
        case 7:
            return 1;
        case 8:
            return 13;
        case 14:
            return 94;
        case 10:
            return 12;
        case 2:
            return 18;
        case 9:
            return 91;
        case 5:
            return 78;
        case 12:
            return 15;
        case 21:
            return 151;
        case 24:
            return 3;
        case 43:
            return 8;
        case 13:
            return 92;
        case 4:
            return 83;
        case 11:
            return 14;
        case 6:
            return 17;
        default:
            return -1;
    }
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

var isAccountBlocked = false;

function getUserBalance() {
    var balDt = window.Locator.user._balance.totalBalance;
    return parseFloat(balDt);//"13.98"
}

function toggleMousemode() {
    document.body.dispatchEvent(new MouseEvent("mousemove"));
    betSlipManager()?.betslip?.activeModule.mouseModeEnabled();
    betSlipManager().betslip.activeModule.enableMouseMode();
}

function getStakeCount() {
    if(betSlipManager().getBetCount === undefined)
        console.log("Возможны изменения на странице")
    return betSlipManager().getBetCount();
}

function getMybetsCount() {
    return Locator.betSlipManager.myBetsManager.myBetsCount;
}
//Селектор указывает ,что ставка поставлена успешно
function hasReceipt() {
    return !!document.querySelector(".lqb-PlaceBetButton_Receipt")
}

//Селектор указывает,что ставка находится в процессе заключения пари
function hasInProgress() {
    return !!document.getElementsByClassName("lqb-QuickBetslip_Processing")[0]
}

async function sleep(msec) {
    return new Promise(resolve => setTimeout(resolve, msec));
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

async function asyncCallbackHot(callbackBoolean, where) {
    return await asyncCallbackLong(callbackBoolean, 200, 50);
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

function parseBalance(){
    if(window?.Locator?.user?._balance?.balanceModelDelegates[0]._text.includes("pyб") || window?.Locator?.user?._balance?.balanceModelDelegates[0]._text.includes("руб")){
        let balanceForReplace = window?.Locator?.user?._balance?.balanceModelDelegates[0].balanceValue.replace(".",",");
        return {
            Amount:+window?.Locator.user?._balance?.totalBalance,
            Currency:"руб"
        }
    }
    return {
        Amount:+window?.Locator.user?._balance?.totalBalance,
        Currency:window?.Locator?.user?._balance?.balanceModelDelegates[0]._text[0]
    }
}

//хоккей
function parseMatchDataHockey() {
    let additionalData = document.getElementsByClassName("lsb-ScoreBasedScoreboard_ScoreContainer ")[0]?.innerText;
    if (!additionalData) {
        additionalData = ""
    } else {
        additionalData = additionalData.replace(/\n/g, ' ');
    }
    let time = {
        MainTime: (document.getElementsByClassName("ml17-IceHockeyScoreboardHeaderCell ml-ScoreboardHeaderCell ")[0]?.textContent) ?? "",
        AdditionalTime: ""
    };

    return {
        AdditionalData: additionalData,
        AdditionalDataSet: "",
        AdditionalDataGame: "",
        AdditionalDataScore: "",
        Time: time
    };
}

//футбол,  баскетбол, теннис(только счет в гейме), гандбол, водное поло, регби, футзал, хоккей, киберфутбол, киберхоккей, кибербаскет
function parseMatchDataSecondary() {
    let additionalData = document.getElementsByClassName("lsb-ScoreBasedScoreboard_ScoreContainer ")[0]?.innerText;
    if (!additionalData) {
        additionalData = ""
    } else {
        additionalData = additionalData.replace(/\n/g, ' ');
    }
    let time = {
        MainTime: (document.getElementsByClassName("ml1-SoccerClock_Clock")[0]?.textContent
            || document.getElementsByClassName("ipe-EventHeader_ClockContainer")[0]?.textContent) ?? "",
        AdditionalTime: ""
    };

    return {
        AdditionalData: additionalData,
        AdditionalDataSet: "",
        AdditionalDataGame: "",
        AdditionalDataScore: "",
        Time: time
    };
}

function parseMatchData_Tennis() {
    let additionalDataSet = (document.getElementsByClassName("ml13-DefaultScores_SetsWrapper")[0]?.innerText?.replace(/\n/g, ' ').split(" ").splice(1)).join(' ') ?? "";
    let additionalDataGame = document.getElementsByClassName("ml13-DefaultScores_SetGroup")[0]?.innerText?.replace(/\n/g, ' ').split(" ").splice(1).join(' ') ?? "";

    return {
        AdditionalData: "",
        AdditionalDataSet: additionalDataSet,
        AdditionalDataGame: additionalDataGame,
        AdditionalDataScore: additionalDataScore,
        Time: {
            MainTime: "",
            AdditionalTime: ""
        }
    };
}


function parseMatchData_BadmintonAndVolleyball() { //бадминтон, волейбол, настольный теннис
    let additionalDataSet = (document.getElementsByClassName("lsb-StandardSetScoreType_Totals ")[0]?.innerText?.replace(/\n/g, ' ')) ?? "";
    let additionalDataScore = document.getElementsByClassName("lsb-StandardSetScoreType_CurrentSetScores ")[0]?.innerText?.replace(/\n/g, ' ') ?? "";

    return {
        AdditionalData: "",
        AdditionalDataSet: additionalDataSet,
        AdditionalDataGame: "",
        AdditionalDataScore: additionalDataScore,
        Time: {
            MainTime: "",
            AdditionalTime: ""
        }
    };
}

function parseMatchData_Snooker() {
    let additionalDataSet = (document.getElementsByClassName("lsb-SnookerScores_FramesWrapper")[0]?.innerText?.replace(/\n/g, ' ')) ?? "";
    let additionalDataScore = document.getElementsByClassName("lsb-SnookerScores_PointsWrapper")[0]?.innerText?.replace(/\n/g, ' ') ?? "";

    return {
        AdditionalData: "",
        AdditionalDataSet: additionalDataSet,
        AdditionalDataGame: "",
        AdditionalDataScore: additionalDataScore,
        Time: {
            MainTime: "",
            AdditionalTime: ""
        }
    };

}

function parseMatchDataUSAFootball() {
    let additionalData = document.getElementsByClassName(" имя_класса_счета")[0]?.innerText ?? ""

    return {
        AdditionalData: additionalData,
        AdditionalDataSet: "",
        AdditionalDataGame: "",
        AdditionalDataScore: "",
        Time: {
            MainTime: (document.getElementsByClassName(" имя_класса_времени")[0]?.textContent) ?? "",
            AdditionalTime: ""
        }
    };

}

function parseMatchDataCyberSport() {
    let first_team = document.getElementsByClassName("lsb-CSGOScoreboard_RoundScore ")[0]?.innerText ?? ""
    let second_team = document.getElementsByClassName("lsb-CSGOScoreboard_RoundScore ")[1]?.innerText ?? ""


    return {
        AdditionalData: first_team + " " + second_team,
        AdditionalDataSet: "",
        AdditionalDataGame: "",
        AdditionalDataScore: "",
        Time: {
            MainTime: (document.getElementsByClassName("csg-Timer_Clock ")[0]?.textContent) ?? "",
            AdditionalTime: ""
        }
    };

}

function parseMatchData() {
    let result;
    let sport = document.getElementsByClassName("ipe-EventHeader_BreadcrumbText")[0]?.innerText ?? ""
    //if( sport.toLowerCase().includes('футбол')){
    switch (sport.toLowerCase()) {
        case 'футбол':
        case 'баскетбол':
        case 'гандбол':
        case 'регби':
        case 'футзал':
        case 'киберфутбол':
        case 'кибербаскет':
            result = parseMatchDataSecondary();
            return result;
        case   "хоккей":
        case   "киберхоккей":
            result = parseMatchDataHockey()
            return result;
        case   "бадминтон":
        case   "волейбол":
            result = parseMatchData_BadmintonAndVolleyball()
            return result;
        case   "теннис":
        case   "кибертеннс":
            result = parseMatchData_Tennis()
            return result;
        case   "снукер":
            result = parseMatchData_Snooker()
            return result;
        case "киберспорт":
            result = parseMatchDataCyberSport()
            return result;
    }

    if (sport.toLowerCase().includes('настольный теннис')) {
        result = parseMatchData_BadmintonAndVolleyball();
        return result;
    }
    if (sport.toLowerCase().includes('водное поло')) {
        result = parseMatchData();
        return result;
    }

    if (sport.toLowerCase().includes('американский футбол')) {
        result = parseMatchDataUSAFootball();
        return result;
    }
    return "";
}
function fireMouseEvent(element, event) {
    var evt = document.createEvent('MouseEvents');
    evt.initEvent(event, true, true);
    element.dispatchEvent(evt);
}