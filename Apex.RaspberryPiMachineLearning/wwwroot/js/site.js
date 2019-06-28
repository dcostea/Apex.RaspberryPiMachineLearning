var isTrained = false;

$(document).ready(function () {

    $("#source").hide();

    $("#train").click(function () {
        $("#source").html("Predicted source: ???");

        startTraining();
        $("#train").hide();
        $("#auto_train").hide();
        $("#source").show();
    });

    $("#auto_train").click(function () {
        $("#source").html("Predicted source: ???");

        startAutoTraining();
        $("#train").hide();
        $("#auto_train").hide();
        $("#source").show();
    });

    const connection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Error)
        .withUrl("http://172.20.10.5:5000/sensor")
        .build();

    connection.on("streamingSource", function (source) {
        console.log("STREAMING SOURCE: " + source);
    });

    connection.on("streamingStopped", function () {
        console.log("STREAMING STOPPED");
    });

    connection.on("streamingStarted", function (source) {
        console.log("STREAMING STARTED");

        connection.stream("SensorsTick").subscribe({
            close: false,
            next: sensors => {
                if (isTrained) {
                    getPrediction(sensors);
                }
            },
            err: err => {
                console.log(err);
            },
            complete: () => {
                console.log("finished streaming");
            }
        });
    });

    connection.start();
});

function startTraining() {
    $.get("/api/sensor/train", function (data, status) {
        
        if (status === "success") {
            isTrained = true;
        }
        else {
            $("#source").html("training failure");                    
        }
    });            
}

function startAutoTraining() {
    $.get("/api/sensor/auto_train", function (data, status) {

        if (status === "success") {
            isTrained = true;
        }
        else {
            $("#source").html("training failure");
        }
    });
}

function getPrediction(sensors) {
    $.get(`/api/sensor/${sensors.luminosity}/${sensors.temperature}/${sensors.infrared}`, function (data, status) {
        if (status === "success") {
            $("#source").html(`Predicted source: ${data}`);
            $("#image_source").attr("src", `images/${data}.png`);                
        }
        else {
            $("#source").html("???");                    
        }
    }); 
}

