<!-- src/routes/home.svelte -->

<script>
  import Ace_Sat from "./ace_sat.svelte";
  import Dis_Sat from "./dis_sat.svelte";
  import { onMount } from "svelte";
  import { Chart } from "chart.js/auto";

  const apiKey = "e650fd6d1ae2872bd20499f9ba3fbb83";
  const apiBaseUrl = "https://app-rssi-new-api-sea-dev.azurewebsites.net";
  // Common API key for both endpoints
  const earthDataApiUrl = apiBaseUrl + "/api/earthdata/ncei";
  const solarWindDataApiUrl = apiBaseUrl + "/api/satellitedata/dscovr";
  // Define the API endpoint for posting tweets
  const tweetApiUrl = apiBaseUrl + "/api/tweets";
  // Store recoonection event info to DB
  const dbFeedbackUrl = apiBaseUrl + "/api/feedback/feedback";

  let displayACESatellite = false;
  let displayDSCOVRSatellite = false;

  function showACESatellite() {
    displayACESatellite = true;
    displayDSCOVRSatellite = false;
  }

  function showDSCOVRSatellite() {
    displayACESatellite = false;
    displayDSCOVRSatellite = true;
  }

  let magneticReconnection = false;

  let earthChart;
  let solarWindChart;

  // Initialize Earth data as an empty object
  let earthData = {};

  // Initialize Sun data as an empty object
  let solarWindData = {};

  // Convert the Earth and Sun data objects into arrays for chart data
  let earthDataLabels = [];
  let earthDataValues = [];
  let solarWindDataLabels = [];
  let solarWindDataValues = [];

  // Chart data for Earth and solar wind

  let earthChartData = {
    labels: earthDataLabels,
    datasets: [
      {
        label: "Vertical",
        data: earthDataValues[0],
        borderColor: "rgba(75, 192, 192, 1)",
        borderWidth: 2,
        fill: false,
      },
      {
        label: "Inclination",
        data: earthDataValues[1],
        borderColor: "rgba(255, 99, 132, 1)",
        borderWidth: 2,
        fill: false,
      },
      {
        label: "Declination",
        data: earthDataValues[2],
        borderColor: "rgba(255, 99, 132, 1)",
        borderWidth: 2,
        fill: false,
      },
    ],
  };

  let solarWindChartData = {
    labels: solarWindDataLabels,
    datasets: [
      {
        label: "bt",
        data: solarWindDataValues[0],
        borderColor: "rgba(54, 162, 235, 1)",
        borderWidth: 2,
        fill: false,
      },
      {
        label: "bzGSM",
        data: solarWindDataValues[1],
        borderColor: "rgba(255, 206, 86, 1)",
        borderWidth: 2,
        fill: false,
      },
      {
        label: "byGSM",
        data: solarWindDataValues[2],
        borderColor: "rgba(75, 192, 192, 1)",
        borderWidth: 2,
        fill: false,
      },
      {
        label: "bxGSM",
        data: solarWindDataValues[3],
        borderColor: "rgba(75, 192, 192, 1)",
        borderWidth: 2,
        fill: false,
      },
    ],
  };

  // Function to check for magnetic reconnection
  function checkMagneticReconnection() {
    const Vertical = earthData.vertical;
    const BzGsm = solarWindData.bzGSM;

    if (Math.abs(Vertical + BzGsm) <= 1000 && BzGsm < 0) {
      // Magnetic reconnection detected
      return true;
    } else {
      return false;
    }
  }

  async function magneticReconnectionDetectionTweet() {
    var msg = "Magnetic reconnection detected!";
    msg += "\n@SpaceApps @NASASocial @NASAEarth";
    // Create the request body
    const requestBody = {
      text: msg,
    };

    try {
      const response = await fetch(tweetApiUrl, {
        method: "POST",
        headers: {
          "x-api-key": `${apiKey}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(requestBody),
      });

      if (response.ok) {
        // Successfully posted the tweet
        console.log("Tweet posted successfully.");
      } else {
        // Handle any errors during the POST request
        console.error("Failed to post the tweet.");
      }
    } catch (error) {
      // Handle network or fetch errors
      console.error("Error while posting the tweet:", error);
    }
  }

  async function saveReconnectionOccurence() {
    
    const requestBody = {
      bt: solarWindData.bt, 
      bxGSM: solarWindData.bxGSM,
      byGSM: solarWindData.byGSM, 
      bzGSM: solarWindData.bzGSM,
      speed: solarWindData.speed,
      density: solarWindData.density,
      temperature: solarWindData.temperature,
    };

    try {
      const response = await fetch(dbFeedbackUrl, {
        method: "POST",
        headers: {
          "x-api-key": `${apiKey}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(requestBody),
      });

      if (response.ok) {
        // Successfully posted the tweet
        console.log("Saved to DB successfully.");
      } else {
        // Handle any errors during the POST request
        console.error("Failed saving to DB!");
      }
    } catch (error) {
      // Handle network or fetch errors
      console.error("Error while saving to DB :", error);
    }
  }

  function updateEarthChart(labels, values) {
    earthChart.data.labels = labels;
    earthChart.data.datasets[0].data = values;
    earthChart.update();
  }

  function updateSolarWindChart(labels, values) {
    solarWindChart.data.labels = labels;
    solarWindChart.data.datasets[0].data = values;
    solarWindChart.update();
  }

  async function fetchEarthData() {
    // Fetch Earth data
    try {
      const earthResponse = await fetch(earthDataApiUrl, {
        headers: {
          "x-api-key": `${apiKey}`,
          "content-type": "application/json",
        },
      });

      if (earthResponse.ok) {
        earthData = await earthResponse.json();

        if (earthDataLabels.length < 3) {
          earthDataLabels.push("Vertical");
          earthDataLabels.push("Inclination");
          earthDataLabels.push("Declination");
        }

        earthDataValues.push(earthData.vertical);
        earthDataValues.push(earthData.inclination);
        earthDataValues.push(earthData.declination);

        // Update the Earth chart with new data
        updateEarthChart(earthDataLabels, earthDataValues);
        earthDataValues = [];

        // console.log(earthData);
      } else {
        console.error("Failed to fetch Earth data from the API.");
      }
    } catch (error) {
      console.error("Error while fetching Earth data:", error);
    }
  }

  async function fetchSolarWindData() {
    // Fetch Solar wind data
    try {
      const solarWindResponse = await fetch(solarWindDataApiUrl, {
        headers: {
          "x-api-key": `${apiKey}`,
          "Content-Type": "application/json",
        },
      });

      if (solarWindResponse.ok) {
        solarWindData = await solarWindResponse.json();

        if (solarWindDataLabels.length < 4) {
          solarWindDataLabels.push("bt");
          solarWindDataLabels.push("bzGSM");
          solarWindDataLabels.push("byGSM");
          solarWindDataLabels.push("bxGSM");
        }

        solarWindDataValues.push(solarWindData.bt);
        solarWindDataValues.push(solarWindData.bzGSM);
        solarWindDataValues.push(solarWindData.byGSM);
        solarWindDataValues.push(solarWindData.bxGSM);


        // Update the Sun chart with new data
        updateSolarWindChart(solarWindDataLabels, solarWindDataValues);
        solarWindDataValues = [];

        // console.log(solarWindData);
      } else {
        console.error("Failed to fetch solar wind data from the API.");
      }
    } catch (error) {
      console.error("Error while fetching solar wind data:", error);
    }
  }

  async function lifeCycleEvent() {
    // Fetch data
    await fetchEarthData();
    await fetchSolarWindData();

    // Check reconnection
    magneticReconnection = checkMagneticReconnection();
    // Take appropriate action
    if (magneticReconnection) {
      /* Not necessarily needed as taken care of in the backend*/
      // await saveReconnectionOccurence();
      // await magneticReconnectionDetectionTweet();
    }
  }

  onMount(async () => {
    await lifeCycleEvent();
    // Fetch new data at 10s interval
    setInterval(lifeCycleEvent, 10000);
  });

  // Initialize solar wind chart
  onMount(() => {
    const solarWindCanvas = document.getElementById("solarWindChart");
    solarWindChart = new Chart(solarWindCanvas, {
      type: "line",
      data: solarWindChartData,
      options: {
        scales: {
          x: {
            display: true, // Display the X-axis
          },
          y: {
            beginAtZero: true,
          },
        },
        plugins: {
          title: {
            display: true,
            text: "Solar Wind Data",
          },
          legend: {
            display: false,
          },
        },
      },
    });

    const earthCanvas = document.getElementById("earthChart");
    earthChart = new Chart(earthCanvas, {
      type: "line",
      data: earthChartData,
      options: {
        scales: {
          x: {
            display: true, // Display the X-axis
          },
          y: {
            beginAtZero: true,
            display: true, // Display the Y-axis
          },
        },
        plugins: {
          title: {
            display: true,
            text: "Geo-magnetic Data",
          },
          legend: {
            display: false,
          },
        },
      },
    });
  });
</script>

<main class="">
      <section class="w-full col-span-4 h-full space-y-10">
<div class="flex gap-x-10">
        <div class="border h-fit p-2 w-full">
        <h2>Geo-magnet</h2>
        <canvas id="earthChart" width="100" height="50" />
      </div>
      <div class="border p-2 h-fit w-full">
        <h2>Solar wind</h2>
        <canvas id="solarWindChart" width="100" height="50" />
      </div>
</div>

      <div class="relative bottom-10">
        {#if magneticReconnection}
          <p class="bullet-point-red">Magnetic Reconnection Detected!</p>
        {:else}
          <p class="bullet-point-green">No Magnetic Reconnection Detected.</p>
        {/if}
      </div>
    </section>
  <div class="navbar">
    <div>
      <button on:click={showACESatellite} class="text-xs p-3">
        ACE Satellite
      </button>
      <button on:click={showDSCOVRSatellite} class="text-xs p-3">
        DSCOVR Satellite
      </button>
    </div>
  </div>

  <!-- Display the S-Satellite component when clicked -->
  <div class=" gap-x-10">
    <div class="w-full h-full flex col-span-8 ">
      {#if displayACESatellite}
        <Ace_Sat />
      {:else}
        <!-- Display the DSCOVR Satellite component when clicked -->
        {#if displayDSCOVRSatellite}
          <Dis_Sat />
        {/if}
      {/if}
    </div>

  </div>

  <!-- Magnetic Reconnection Detection -->
</main>

<style>
  /* Your CSS styles for home.svelte */

  .bullet-point-green {
    position: relative;
    padding-left: 20px; /* Adjust the spacing as needed */
  }

  .bullet-point-red {
    position: relative;
    padding-left: 20px; /* Adjust the spacing as needed */
  }

  .bullet-point-green::before {
    content: "";
    display: inline-block;
    width: 10px; /* Adjust the size of the bullet point */
    height: 10px; /* Adjust the size of the bullet point */
    background-color: rgb(172, 228, 20);
    border-radius: 50%;
    position: absolute;
    left: 0;
    top: 50%;
    transform: translateY(-50%);
    animation: glow 1s infinite; /* Add a glow animation */
  }

  .bullet-point-red::before {
    content: "";
    display: inline-block;
    width: 10px; /* Adjust the size of the bullet point */
    height: 10px; /* Adjust the size of the bullet point */
    background-color: red;
    border-radius: 50%;
    position: absolute;
    left: 0;
    top: 50%;
    transform: translateY(-50%);
    animation: glow 1s infinite; /* Add a glow animation */
  }

  @keyframes glow {
    0% {
      box-shadow: 0 0 5px rgb(172, 228, 20);
    }
    50% {
      box-shadow: 0 0 20px rgb(172, 228, 20);
    }
    100% {
      box-shadow: 0 0 5px rgb(172, 228, 20);
    }
  }
  .navbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  /* Button code */
  button,
  button::after {
    font-size: 20px;
    border: none;
    border-radius: 5px;
    color: white;
    background-color: transparent;
    position: relative;
    background-color: transparent;
    border: 1px solid rgb(0, 255, 213);
    box-shadow: 0px 5px 10px -5px rgb(0, 255, 213);
    background-color: #132e27;
  }

  button:hover::after {
    text-shadow: black;

    text-shadow: #1df2f0, #e94be8;
    background-color: transparent;
    border: 3px solid rgb(0, 255, 213);
  }

  button:hover {
    text-shadow:
      -1px -1px 0px #1df2f0,
      1px 1px 0px #e94be8;
  }
</style>
