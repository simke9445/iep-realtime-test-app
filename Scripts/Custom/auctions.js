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

            var auctionId = $(this).data("id");

            $.connection.auctionTicker.server.bid(auctionId);

        });
});

var ticker, rowTemplate, $auctionsTable = $("#auctionsTable"), $auctionsTableBody = $auctionsTable.find(".auctionTable");


$(function() {
    
    ticker = $.connection.auctionTicker;
    rowTemplate = '<div class="col-md-3"> ' +
                    '<div class="panel panel-default panel-auction" data-id={Id}>' +
                        '<div class="panel-heading text-center"> <a href="Auction/{Id}"> {ProductName} </a> </div>' +
                        '<div class="panel-body text-center">' +
                            '<img src={Image} class="img-rounded auction-img" />' +
                            '<table class="table">' +
                                '<tr><td>' +
                                    '<p class="price">${StartingPrice}</p>' +
                                '</td></tr>' +
                                '<tr><td>' +
                                    '<p class="last-bid-user">{LastBidUserUserName}</p>' +
                                '</td></tr>' +
                                '<tr><td>' +
                                    '<p class="time">{Time}</p>' +
                                '</td></tr>' +
                                '<tr><td>' +
                                    '<p class="state">{State}</p> ' +
                                '</td></tr>' +
                                '<tr><td class="text-center">' +
                                    '<div class="btn btn-primary extend" data-id={Id}>Extend</div>' +
                                '</td></tr>' +
                            '</table>' +
                        '</div>' +
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
                if (loadedAuctions.length < 11) {
                    var lastBidUserName = dom.find(".last-bid-user"),
                        state = dom.find(".state"),
                        time = dom.find(".time");
                    if (lastBidUserName.text() === "{LastBidUserUserName}") {
                        lastBidUserName.text("No current bidder");
                    }

                    switch(state.text()) {
                        case "0":
                            state.text("Draft");
                            break;
                        case "1":
                            state.text("Ready");
                            break;
                        case "2":
                            state.text("Open");
                            break;
                        case "3":
                            state.text("Sold");
                            break;
                        case "4":
                            state.text("Expired");
                            break;
                    }

                    var minutes = Math.floor(this.Time / 60),
                        seconds = this.Time % 60;

                    var finalTime = (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);
                    time.text(finalTime);

                    $auctionsTableBody.append(dom);
                }
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

                    var minutes = Math.floor(this.auction.Time / 60),
                        seconds = this.auction.Time % 60;
                    var finalTime = (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);
                    this.dom.find(".time").text(finalTime);
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
            currentAuction.auction = extendedAuction;
            currentAuction.dom.find(".last-bid-user").html(currentAuction.auction.LastBidUser.UserName);
            currentAuction.dom.find(".price").html("$" + currentAuction.auction.StartingPrice);

            var minutes = Math.floor(currentAuction.auction.Time / 60),
                seconds = currentAuction.auction.Time % 60;
            var finalTime = (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);
            console.log(finalTime);
            currentAuction.dom.find(".time").text(finalTime);


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


    ticker.client.openAuction = function(openedAuction) {
        
        var currentAuction = loadedAuctions.find(function (auction) {
            return auction.auction.Id === openedAuction.Id;
        });

        if (currentAuction != null) {
            currentAuction.auction = openedAuction;
            currentAuction.dom.find(".state").text("Open");
            currentAuction.dom.find(".extend").show();
        }
    }


    // Start the connection
    $.connection.hub.start().done(init);
});