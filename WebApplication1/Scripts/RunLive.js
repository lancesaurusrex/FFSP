$(function () {

    // A simple templating method for replacing placeholders enclosed in curly braces.
    if (!String.prototype.supplant) {
        String.prototype.supplant = function (o) {
            return this.replace(/{([^{}]*)}/g,
                function (a, b) {
                    var r = o[b];
                    return typeof r === 'string' || typeof r === 'number' ? r : a;
                }
            );
        };
    }


    var liveNFLGame = $.connection.nFLLiveUpdateHub,
        $playerTable = $('#playerTable'),
        $playerTableBody = $playerTable.find('tbody'),
        rowTemplate = '<tr id="{Id}"><td>{Name}</td><td>{Team}</td><td>{Pts}</td></tr>';

    function formatPlayer(NFLPlayer) {
        return $.extend(NFLPlayer, {
            Id: NFLPlayer.id,
            Name: NFLPlayer.name,
            Team: NFLPlayer.team,
            Pts: NFLPlayer.currentPts   
        });
    }


    function init() {
        liveNFLGame.server.getAllPlayers().done(function (players) {
            $playerTableBody.empty();
            $.each(players, function () {
                var NFLPlayer = formatPlayer(this);
                $playerTableBody.append(rowTemplate.supplant(NFLPlayer));
            });
        });
    }

    // Add a client-side hub method that the server will call
    liveNFLGame.client.updatePlayers = function (NFLPlayer) {
        var displayPlayer = formatPlayer(NFLPlayer),
            $row = $(rowTemplate.supplant(displayPlayer));
        var x = document.getElementById("playerTable");
        $playerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .replaceWith($row);
    }

    // Start the connection
    $.connection.hub.start().done(init);
            
});