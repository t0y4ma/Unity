handlers.UpdateScoreAndRanking = function (args, context) {

    var MAX_RANK = 300;

    var playerId = currentPlayerId;
    var score = args.score;
    var time = args.time;
    var moveCount = args.moveCount;
    var name = args.name || "Player";

    // ---- バリデーション ----
    if (score == null || time == null || moveCount == null) {
        return { error: "invalid args" };
    }

    if (score < 0 || time < 0 || moveCount < 0) {
        return { error: "invalid values" };
    }

    // ---- 取得（TitleData）----
    var res = server.GetTitleData({
        Keys: ["leaderboard"]
    });

    var leaderboard = [];

    if (res.Data && res.Data["leaderboard"]) {
        try {
            leaderboard = JSON.parse(res.Data["leaderboard"]);
        } catch (e) {
            leaderboard = [];
        }
    }

    // ---- 既存削除 ----
    var newList = [];
    for (var i = 0; i < leaderboard.length; i++) {
        if (leaderboard[i].playerId !== playerId) {
            newList.push(leaderboard[i]);
        }
    }
    leaderboard = newList;

    // ---- 比較関数 ----
    function isBetter(a, b) {
        if (a.moveCount !== b.moveCount) return a.moveCount < b.moveCount;
        if (a.score !== b.score) return a.score > b.score;
        if (a.time !== b.time) return a.time < b.time;
        return a.playerId < b.playerId;
    }

    var newEntry = {
        playerId: playerId,
        name: name,
        score: score,
        time: time,
        moveCount: moveCount
    };

    // ---- 挿入 ----
    var inserted = false;

    for (var i = 0; i < leaderboard.length; i++) {
        if (isBetter(newEntry, leaderboard[i])) {
            leaderboard.splice(i, 0, newEntry);
            inserted = true;
            break;
        }
    }

    if (!inserted) {
        leaderboard.push(newEntry);
    }

    // ---- サイズ制限 ----
    if (leaderboard.length > MAX_RANK) {
        leaderboard = leaderboard.slice(0, MAX_RANK);
    }

    // ---- 保存（TitleData）----
    server.SetTitleData({
        Key: "leaderboard",
        Value: JSON.stringify(leaderboard)
    });

    // ---- 順位 ----
    var rank = -1;
    for (var i = 0; i < leaderboard.length; i++) {
        if (leaderboard[i].playerId === playerId) {
            rank = i + 1;
            break;
        }
    }

    return {
        rank: rank,
        inTop: rank !== -1
    };
};


handlers.GetTopRanking = function (args, context) {

    var res = server.GetTitleData({
        Keys: ["leaderboard"]
    });

    if (!res.Data || !res.Data["leaderboard"]) {
        return [];
    }

    try {
        return JSON.parse(res.Data["leaderboard"]);
    } catch (e) {
        return [];
    }
};