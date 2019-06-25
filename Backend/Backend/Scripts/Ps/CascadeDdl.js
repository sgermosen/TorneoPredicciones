 
    $(document).ready(function () {
        $(".MasterDdl").change(function () {
            $(".DetailDdl").empty();
            $.ajax({
                type: "POST",
                url: Url,
                dataType: "json",
                data: { leagueId: $(".MasterDdl").val() },
                success: function (teams) {
                    $.each(teams,
                        function (i, team) {
                            $(".DetailDdl").append('<option value="' + team.TeamId + '">' + team.Name + "</option>");
                        });
                },
                error: function (ex) {
                    alert("Error al intentar traer los quipos." + ex);
                }
            });
            return false;
        });

        $("#LocalLeagueId").change(function () {
            $("#LocalId").empty();
            $.ajax({
                type: "POST",
                url: Url,
                dataType: "json",
                data: { leagueId: $("#LocalLeagueId").val() },
                success: function (teams) {
                    $.each(teams,
                        function (i, team) {
                            $("#LocalId").append('<option value="' + team.TeamId + '">' + team.Name + "</option>");
                        });
                },
                error: function (ex) {
                    alert("Error al intentar traer los quipos." + ex);
                }
            });
            return false;
        });
        $("#VisitorLeagueId").change(function () {
            $("#VisitorId").empty();
            $.ajax({
                type: "POST",
                url: Url,
                dataType: "json",
                data: { leagueId: $("#VisitorLeagueId").val() },
                success: function (teams) {
                    $.each(teams,
                        function (i, team) {
                            $("#VisitorId").append('<option value="' + team.TeamId + '">' + team.Name + "</option>");
                        });
                },
                error: function (ex) {
                    alert("Error al intentar traer los quipos." + ex);
                }
            });
            return false;
        });
        $("#FavoriteLeagueId").change(function () {
            $("#FavoriteTeamId").empty();
            $.ajax({
                type: "POST",
                url: Url,
                dataType: "json",
                data: { leagueId: $("#FavoriteLeagueId").val() },
                success: function (teams) {
                    $.each(teams,
                        function (i, team) {
                            $("#FavoriteTeamId").append('<option value="' + team.TeamId + '">' + team.Name + "</option>");
                        });
                },
                error: function (ex) {
                    alert("Error al intentar traer los quipos." + ex);
                }
            });
            return false;
        });
        //$("#").change(function () {
        //    $("#").empty();
        //    $.ajax({
        //        type: "POST",
        //        url: Url,
        //        dataType: "json",
        //        data: { leagueId: $("#VisitorLeagueId").val() },
        //        success: function (teams) {
        //            $.each(teams,
        //                function (i, team) {
        //                    $("#VisitorId").append('<option value="' + team.TeamId + '">' + team.Name + "</option>");
        //                });
        //        },
        //        error: function (ex) {
        //            alert("Error al intentar traer los quipos." + ex);
        //        }
        //    });
        //    return false;
        //});
    });
 