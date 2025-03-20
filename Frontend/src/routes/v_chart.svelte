<script>
  import { onMount } from "svelte";
  import Chart from "chart.js/auto";
  import Analytics from "./analytics.svelte";

  const apiKey = "e650fd6d1ae2872bd20499f9ba3fbb83";
  const baseApiUrl = "https://app-rssi-new-api-sea-dev.azurewebsites.net";
  const chartDataUrl = baseApiUrl + "/api/archive/get";

  let orientationChart;
  let magnetometerChart;

  let chart_init = {
    bt: [],
    bx_gsm: [],
    by_gsm: [],
    bz_gsm: [],
    intensity: [],
    inclination: [],
    declination: []
  };


  // Function to fetch chart data
  async function fetchChartData() {
    console.log("Fetching chart data");
    const response = await fetch(chartDataUrl, {
      headers: {
        "x-api-key": `${apiKey}`,
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      chart_init = await response.json();
      console.log(chart_init);
    } else {
      console.error("Failed to fetch chart data!");
    }
  }

  // Helper function to get the last 5 months' names
  const getLastFiveMonths = () => {
    const months = ["Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"];
    const currentDate = new Date();
    const lastFiveMonths = [];

    for (let i = 4; i >= 0; i--) {
      const pastDate = new Date(currentDate.getFullYear(), currentDate.getMonth() - i);
      lastFiveMonths.push(months[pastDate.getMonth()]);
    }

    return lastFiveMonths;
  };

  onMount(async () => {

    await fetchChartData(); // Fetch the chart data first

    const magnetometerCtx = magnetometerChart.getContext("2d");
    const orientationCtx = orientationChart.getContext("2d");

    // Data for the magnetometer chart
    const magnetometerData = {
      labels: getLastFiveMonths(),
      datasets: [
        {
          label: "Bt",
          data: chart_init.bt,
          borderColor: "#94D2BD",
          borderWidth: 2,
          fill: false,
        },
        {
          label: "BzGSM",
          data: chart_init.bz_gsm,
          borderColor: "#EE9B00",
          borderWidth: 2,
          fill: false,
        },
        {
          label: "ByGSM",
          data: chart_init.by_gsm,
          borderColor: "#FB3602",
          borderWidth: 2,
          fill: false,
        },
        {
          label: "BxGSM",
          data: chart_init.bx_gsm,
          borderColor: "#AF1724",
          borderWidth: 2,
          fill: false,
        },
      ],
    };

    // Data for the orientation chart
    const orientationData = {
      labels: getLastFiveMonths(),
      datasets: [
        {
          label: "Intensity",
          data: chart_init.intensity,
          borderColor: "#EE9B00",
          borderWidth: 1,
          fill: false,
        },
        {
          label: "Inclination",
          data: chart_init.inclination,
          borderColor: "#0A9396",
          borderWidth: 2,
          fill: false,
        },
        {
          label: "Declination",
          data: chart_init.declination,
          borderColor: "#94D2BD",
          borderWidth: 2,
          fill: false,
        },
      ],
    };

    // Adjust the chart height for larger charts
    const chartHeight = 600;

    const commonOptions = {
      maintainAspectRatio: true, // Enable aspect ratio
    };

    const magnetometerConfig = {
      type: "line",
      data: magnetometerData,
      options: {
        ...commonOptions,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Magnetometer Data",
            },
          },
          x: {
            title: {
              display: true,
              text: "Month",
            },
          },
        },
      },
    };

    const orientationConfig = {
      type: "line",
      data: orientationData,
      options: {
        ...commonOptions,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Orientation Data",
            },
          },
          x: {
            title: {
              display: true,
              text: "Month",
            },
          },
        },
      },
    };

    const resizeCharts = () => {
      magnetometerChart.height = chartHeight;
      orientationChart.height = chartHeight;
    };

    new Chart(magnetometerCtx, magnetometerConfig);
    new Chart(orientationCtx, orientationConfig);

    // Resize charts initially
    resizeCharts();
  });
</script>

<br />

<main class="">
  <div class="grid grid-cols-2 gap-x-10">
    <div class="border border-[#ffffff4d]/30">
      <canvas
        bind:this={magnetometerChart}
        id="magnetometerChart"
        style="box-shadow: 0px 8px 15px -10px rgb(0, 255, 213);"
      ></canvas>
    </div>
    <div class="border border-[#ffffff4d]/30">
      <canvas
        bind:this={orientationChart}
        id="orientationChart"
        class="border-b border-[#1df2f0]/70"
        style="box-shadow: 0px 8px 15px -10px rgb(0, 255, 213);"
      ></canvas>
    </div>
  </div>
  <div class="analytics"><Analytics /></div>
</main>
