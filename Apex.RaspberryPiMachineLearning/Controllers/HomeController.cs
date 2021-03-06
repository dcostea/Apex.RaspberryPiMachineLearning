﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Apex.RaspberryPiMachineLearning;
using RaspberryPiMachineLearning.MachineLearning;
using Apex.RaspberryPiMachineLearning.Helpers;

namespace RaspberryPiMachineLearning.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly RaspberryPiSettings raspberryPiSettings;
        private readonly CognitiveServicesSettings cognitiveServicesSettings;

        public HomeController(IOptions<RaspberryPiSettings> raspberryPiSettings, IOptions<CognitiveServicesSettings> cognitiveServicesSettings)
        {
            this.raspberryPiSettings = raspberryPiSettings.Value;
            this.cognitiveServicesSettings = cognitiveServicesSettings.Value;
        }

        [HttpGet("api/sensor/train")]
        public IActionResult SensorTrain()
        {
            SensorModel.Train();

            return Ok();
        }

        [HttpGet("api/sensor/auto_train")]
        public IActionResult SensorTrainAuto()
        {
            SensorModel.AutoTrain();

            return Ok();
        }

        [HttpGet("api/sensor/{lux}/{temp}/{infra}")]
        public IActionResult SensorPredict(float lux, float temp, float infra)
        {
            var prediction = SensorModel.Predict(lux, temp, infra);

            return Ok(prediction);
        }

        [HttpGet("api/sensor/talk/{text}")]
        public IActionResult TalkAsync(string text)
        {
            var speech = new SpeechHelpers(cognitiveServicesSettings);
            Task.Run(() => speech.Speak(text));

            return Ok();
        }

        [HttpGet("api/sensor/listen")]
        public async Task<IActionResult> Listen()
        {
            var speech = new SpeechHelpers(cognitiveServicesSettings);
            await speech.Listen();

            return Ok();
        }
    }
}
