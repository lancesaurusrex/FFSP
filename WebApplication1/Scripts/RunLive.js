$(function () {
    //pass game id from html to js, might not be necessary if games are run through signalr
    var $vars = $('#RunLive\\.js').data();
    //alert($vars.gid);  //displays on screen game id

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
        $homePlayerTable = $('#homePlayerTable'),
        $awayPlayerTable = $('#awayPlayerTable'),
        $homePlayerTableBody = $homePlayerTable.find('tbody'),
        $awayPlayerTableBody = $awayPlayerTable.find('tbody'),
        rowTemplate = '<tr id="{Id}"><td>{Name}</td><td>{Team}</td><td>{TotalPts}</td><td>{PYds}</td><td>{PTds}</td><td>{PInts}</td><td>{RuYds}</td><td>{RuTds}</td><td>{ReYds}</td><td>{ReTds}</td></tr>';

    function formatPlayer(NFLPlayer) {
        return $.extend(this, {
            Id: NFLPlayer.id,
            Name: NFLPlayer.name,
            Team: NFLPlayer.team,
            TotalPts: NFLPlayer.currentPts,
            PYds: NFLPlayer.PassingStats.yds,
            PTds: NFLPlayer.PassingStats.tds,
            PInts: NFLPlayer.PassingStats.ints,
            RuYds: NFLPlayer.RushingStats.yds,
            RuTds: NFLPlayer.RushingStats.tds,
            ReYds: NFLPlayer.ReceivingStats.yds,
            ReTds: NFLPlayer.ReceivingStats.tds
        });
    }

    //find table row by id in html
    //var table = document.getElementById("tableId");
    //var rowIndex = document.getElementById("b").rowIndex;
    //table.deleteRow(rowIndex);

    function init() {
        liveNFLGame.server.getAllHomePlayers($vars.gid).done(function (players) {
            $homePlayerTableBody.empty();
            $.each(players, function () {
                var NFLPlayer = formatPlayer(this);
                $homePlayerTableBody.append(rowTemplate.supplant(NFLPlayer));
            });
        });

        liveNFLGame.server.getAllAwayPlayers($vars.gid).done(function (players) {
            $awayPlayerTableBody.empty();
            $.each(players, function () {
                var NFLPlayer = formatPlayer(this);
                $awayPlayerTableBody.append(rowTemplate.supplant(NFLPlayer));
            });
        });
    }

    // Add a client-side hub method that the server will call
    //Need to add away update   
    liveNFLGame.client.updatePlayers = function (NFLPlayer) {
        var displayPlayer = formatPlayer(NFLPlayer),
            $row = $(rowTemplate.supplant(displayPlayer));

        //$("tr:not(:empty)").css("background-color", "black");
        $homePlayerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .replaceWith($row);
        $awayPlayerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .replaceWith($row);
        //$rh = $homePlayerTableBody.find('tr[id=' + NFLPlayer.id + ']');
        //$rh.css('background-color', 'red');
        //$ra = $awayPlayerTableBody.find('tr[id=' + NFLPlayer.id + ']');
        //$ra.css('background-color', 'blue');
       // document.getElementById("homePlayerTable").style.backgroundColor = '#FF0000';
        //document.getElementById("awayPlayerTable").style.backgroundColor = '#FFA500';
       // document.getElementById('id').style.backgroundColor = "Red";
        //document.getElementById('Id').style.backgroundColor = "Red";
        //var a = document.getElementById("homePlayerTable");
        //var b =a.getElementsByTagName(Id);
        var a = $awayPlayerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .css({ backgroundColor: 'red' })
            .show()
        setTimeout(function () {
            a.css({ backgroundColor: '' });
        }, 2000);

        var b = $homePlayerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .css({ backgroundColor: 'blue' })
            .show()
        setTimeout(function () {
            b.css({ backgroundColor: '' });
        }, 2000);
    }

    liveNFLGame.client.updatePlay = function (play) {
        $("#plays").html(play);
    }

    liveNFLGame.client.updatePlay2 = function (play2) {
        $("#plays2").html(play2);
    }

    // Start the connection
    $.connection.hub.start().done(init); 
    
});