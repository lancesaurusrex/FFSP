﻿$(function () {
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
        rowTemplate = '<tr id="{Id}"><td>{Name}</td><td>{Team}</td><td>{TotalPts}</td><td>{PYds}</td><td>{PTds}</td><td>{RuYds}</td><td>{RuTds}</td><td>{ReYds}</td><td>{ReTds}</td></tr>';

    function formatPlayer(NFLPlayer) {
        return $.extend(NFLPlayer, {
            Id: NFLPlayer.id,
            Name: NFLPlayer.name,
            Team: NFLPlayer.team,
            TotalPts: NFLPlayer.currentPts,
            PYds: NFLPlayer.PassingStats.PassYds,
            PTds: NFLPlayer.PassingStats.PassTds,
            RuYds: RushingStats.RushYds,
            RuTds: NFLPlayer.RushingStats.RushTds,
            ReYds: NFLPlayer.ReceivingStats.RecYds,
            ReTds: NFLPlayer.ReceivingStats.RecTds
        });
    }


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
        var x = document.getElementById("homePlayerTable");
        $homePlayerTableBody.find('tr[id=' + NFLPlayer.id + ']')
            .replaceWith($row);
    }

    liveNFLGame.client.stopClient = function () {
        $.connection.hub.stop();
    };

    // Start the connection
    $.connection.hub.start().done(init); 
    
});