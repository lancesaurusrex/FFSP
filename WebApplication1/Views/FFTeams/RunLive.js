$(function () {

    var liveNFLGame = $.connection.nFLLiveUpdateHub, // the generated client-side hub proxy
        $playerTable = $('#playerTable'),
        $playerTableBody = $playerTable.find('tbody'),
        rowTemplate = '<tr data-symbol="{Symbol}"><td>{Symbol}</td><td>{Price}</td><td>{DayOpen}</td><td>{Direction} {Change}</td><td>{PercentChange}</td></tr>';

    function formatPlayer(player) {
        return $.extend(player, {
            Price: stock.Price.toFixed(2),
            PercentChange: (stock.PercentChange * 100).toFixed(2) + '%',
            Direction: stock.Change === 0 ? '' : stock.Change >= 0 ? up : down
        });
    }

    function init() {
        liveNFLGame.server.getAllPlayers().done(function (players) {
            $playerTableBody.empty();
            $.each(players, function () {
                var player = formatPlayer(this);
                $playerTableBody.append(rowTemplate.supplant(player));
            });
        });
    }

    // Add a client-side hub method that the server will call
    liveNFLGame.client.updatePlayers = function (player) {
        var displayPlayer = formatPlayer(player),
            $row = $(rowTemplate.supplant(displayPlayer));

        $playerTableBody.find('tr[data-symbol=' + stock.Symbol + ']')
            .replaceWith($row);
    }

    // Start the connection
    $.connection.hub.start().done(init);

});