var loadedAuctions = [];


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

$(function() {
    $(document).on('click', '.extend',
        function () {
            var userTokenStashSize = parseInt($("#userTokenStashSize").text());
            if (userTokenStashSize <= 0 || isNaN(userTokenStashSize)) {
                return;
            }

            var auctionJson = { 'id': $(this).closest("[data-id]").attr("data-id") };

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: '/Auction/ExtendAuction',
                data: JSON.stringify(auctionJson),
                dataType: "json",
                success: function (data) {
                    $("#userTokenStashSize").text(userTokenStashSize - 1);
                    console.log(data.status + " " + data.message);
                },
                error: function(xhr, err) {
                    console.log(xhr + " " + err);
                }
            });
        });
});

var ticker, rowTemplate, $auctionsTable = $("#auctionsTable"), $auctionsTableBody = $auctionsTable.find(".auctionTable");

$(function() {
    
    ticker = $.connection.auctionTicker;
    rowTemplate = '<div class="col-md-3"> ' +
                    '<div class="panel panel-default" data-id={Id}>' +
                        '<div class="panel-heading"> <a href="Auction/{Id}"> Auction: {ProductName} </a> </div>' +
                        '<div class="panel-body text-center">' +
                            '<img src={Image} class="img-rounded" />' +
                            '<p class="price"> {StartingPrice} </p>' +
                            '<p class="time"> {Time} </p>' +
                            '<p class="last-bid-user"> {LastBidUserUserName} </p>' +
                            '<p class="state"> {State} </p> ' +
                        '</div>' +
                        '<div class="panel-footing text-center"> ' +
                            '<button class="btn btn-primary extend">Extend</button>' +
                        '</div> ' +
                    '</div> ' +
                '</div>';

    function init() {
        ticker.server.getAllAuctions().done(function (auctions) {
            $auctionsTableBody.empty();
            console.log(auctions);
            $.each(auctions, function () {
                var dom = $($.parseHTML(rowTemplate.supplant(this)));
                loadedAuctions.push({
                    'auction': this,
                    'dom': dom
                });
                $auctionsTableBody.append(dom);
            });

            var defaultPaginationOptions = {
                totalPages: Math.max(Math.ceil(loadedAuctions.length / 10), 1),
                visiblePages: Math.min(Math.max(Math.ceil(loadedAuctions.length / 10), 1), 5),
                onPageClick: function (event, page) {
                    $auctionsTableBody.empty();

                    var startingIndexForPage = (page - 1) * 10;

                    for (var i = startingIndexForPage; i < loadedAuctions.length; i++) {
                        $auctionsTableBody.append(loadedAuctions[i].dom);
                    }

                }
            };

            $('#gallery').twbsPagination(defaultPaginationOptions);
            
        });
    }

   
    ticker.client.tick = function() {
        $.each(loadedAuctions,
            function () {
                if (this.auction.State === 2) {
                    this.dom.find(".time").html(this.auction.Time - 1);
                    this.auction.Time = this.auction.Time - 1;
                }
            });
    };

    ticker.client.newAuction = function (auction) {
        var dom = $($.parseHTML(rowTemplate.supplant(auction)));
        loadedAuctions.push({
            'auction': auction,
            'dom': dom
        });
        $auctionsTableBody.append(dom);
    };

    ticker.client.extendAuction = function (extendedAuction, extendPeriod) {
        var currentAuction = loadedAuctions.find(function(auction) {
            return auction.auction.Id === extendedAuction.Id;
        });

        if (currentAuction != null) {
            console.log(extendedAuction);
            currentAuction.auction = extendedAuction;
            currentAuction.dom.find(".last-bid-user").html(currentAuction.auction.LastBidUser.UserName);
            currentAuction.dom.find(".time").html(currentAuction.auction.Time);
            currentAuction.dom.find(".price").html(currentAuction.auction.StartingPrice);
        }
    };

    ticker.client.removeAuction = function(removedAuction) {
        var currentAuction = loadedAuctions.find(function (auction) {
            return auction.auction.Id === removedAuction.Id;
        });

        if (currentAuction != null) {
            console.log("Removed auction with name " + currentAuction.auction.ProductName);
            currentAuction.dom.remove();
            var index = loadedAuctions.map(function (e) { return e.auction.Id; }).indexOf(currentAuction.Id);
            if (index !== -1) {
                console.log("Removed at index " + index);
                loadedAuctions.splice(index, 1);
            }
        }
    }

    // Start the connection
    $.connection.hub.start().done(init);
});