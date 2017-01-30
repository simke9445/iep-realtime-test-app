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
            var auctionId = $(this).data("id");

            $.connection.auctionTicker.server.bid(auctionId);

        });
});

var ticker, rowTemplate, $auctionsTableBody = $("#auctionTable");


var auctionsPerPage = 10;


function updateAuctionDom(dom, auction) {
    if (auction.lastBidUser != null) {
        dom.find(".last-bid-user").html(auction.LastBidUser.UserName);
    }
    dom.find(".price").html("$" + auction.StartingPrice);

    var state = dom.find(".state");

    switch(auction.State) {
        case 0:
            state.text("Draft");
            break;
        case 1:
            state.text("Ready");
            break;
        case 2:
            state.text("Open");
            break;
        case 3:
            state.text("Sold");
            break;
        case 4:
            state.text("Expired");
            break;
    }

    var bidDom = dom.find(".extend");

    if (auction.State === 2) {
        bidDom.show();
    } else {
        bidDom.hide();
    }

    

    dom.find(".product").html(auction.ProductName);

    var minutes = Math.floor(auction.Time / 60),
        seconds = auction.Time % 60;
    var finalTime = (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);
    dom.find(".time").text(finalTime);
}


$(function() {
    
    ticker = $.connection.auctionTicker;
    rowTemplate = '<div class="col-md-3"> ' +
                    '<div class="panel panel-primary panel-auction" data-id={Id}>' +
                        '<div class="panel-heading text-center"> <a class="product" href="Auction/{Id}"> {ProductName} </a> </div>' +
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
                                    '<div class="btn btn-primary extend" data-id={Id}>Bid Now</div>' +
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

                var lastBidUserName = dom.find(".last-bid-user"),
                        state = dom.find(".state"),
                        time = dom.find(".time");
                if (lastBidUserName.text() === "{LastBidUserUserName}") {
                    lastBidUserName.text("No current bidder");
                }

                switch (state.text()) {
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

                if (loadedAuctions.length < auctionsPerPage + 1) {

                    $auctionsTableBody.append(dom);
                }
            });

            var defaultPaginationOptions = {
                totalPages: Math.max(Math.ceil(loadedAuctions.length / auctionsPerPage), 1),
                visiblePages: Math.min(Math.max(Math.ceil(loadedAuctions.length / auctionsPerPage), 1), 5),
                onPageClick: function (event, page) {
                    $auctionsTableBody.empty();

                    var startingIndexForPage = (page - 1) * auctionsPerPage;

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


    ticker.client.updateAuction = function(updatedAuction) {
        var currentAuction = loadedAuctions.find(function(auction) {
            return auction.auction.Id === updatedAuction.Id;
        });

        if (currentAuction != null) {
            currentAuction.auction = updatedAuction;
            updateAuctionDom(currentAuction.dom, currentAuction.auction);
        }
    }

        // Start the connection
        $.connection.hub.start().done(init);
    });