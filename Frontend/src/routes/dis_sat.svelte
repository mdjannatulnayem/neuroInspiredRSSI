<!-- src/routes/dis_sat.svelte -->

<script>
  import { onMount } from "svelte";

  const apiKey = "e650fd6d1ae2872bd20499f9ba3fbb83";
  const baseApiUrl = "https://app-rssi-new-api-sea-dev.azurewebsites.net";
  const earthApiUrl = baseApiUrl + "/api/earthdata/ncei";
  const solarWindApiUrl = baseApiUrl + "/api/satellitedata/dscovr";

  let earthData = {};
  let solarWindData = {};

  // Function to fetch Earth data from the API
  async function fetchEarthData() {
    console.log("Fetching geo-magnetic data.");
    const response = await fetch(earthApiUrl, {
      headers: {
        "x-api-key": `${apiKey}`,
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      earthData = await response.json();
    } else {
      console.error("Failed to fetch Earth data from the API.");
    }
  }

  // Function to fetch Sun data from the API
  async function fetchSolarWindData() {
    console.log("Fetching solar wind data from DSCOVR.");
    const response = await fetch(solarWindApiUrl, {
      headers: {
        "x-api-key": apiKey,
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      solarWindData = await response.json();
    } else {
      console.error("Failed to fetch Sun data from the API.");
    }
  }

  onMount(async () => {
    // Fetch Earth and Sun data
    // when the component is mounted
    await fetchEarthData();
    await fetchSolarWindData();
    // fetch dscovr data every 10s
    setInterval(fetchSolarWindData, 10000);
  });
</script>

<main class=" w-full">
  <!-- Left Floating Items -->
  <!--  -->

  <h1 class="mt-10 text-xs border-b border-white/30">
    DSCOVR SATELLITE INFORMATIONS
  </h1>

  <div style=" font-size:16px" class="w-full mt-5 flex">
    <div class="flex w-full space-x-5">
      <div
        class="w-full p-[2px]"
        style="background: linear-gradient( -135deg, transparent 20px, #ffffff4d 0);"
      >
        <div
          class="h-full p-2 grid items-center"
          style="background: linear-gradient( -135deg, transparent 20px, #161E1C 0);"
        >
          <h4
            class="p-2"
            style="background: linear-gradient( -135deg, transparent 20px, #FA8322 0);"
          >
            Solar wind
          </h4>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">bx_gsm:</span>
            <span class="status-text">
              <span class="bx-gsm-value">{solarWindData.bxGSM}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">by_gsm:</span>
            <span class="status-text">
              <span class="by-gsm-value">{solarWindData.byGSM}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">bz_gsm:</span>
            <span class="status-text">
              <span class="bz-gsm-value">{solarWindData.bzGSM}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">bt:</span>
            <span class="status-text">
              <span class="bt-value">{solarWindData.bt}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">proton speed:</span>
            <span class="status-text">
              <span class="bt-value">{solarWindData.speed}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">density:</span>
            <span class="status-text">
              <span class="bt-value">{solarWindData.density}</span>
            </span>
          </div>

          <div class="status-icon">
            <span class="square" />
            <span class="status-label">temperature:</span>
            <span class="status-text">
              <span class="bt-value">{solarWindData.temperature}</span>
            </span>
          </div>
        </div>
      </div>

      <div
        class="w-full p-[2px]"
        style="background: linear-gradient( -135deg, transparent 20px, #ffffff4d 0);"
      >
        <div
          class="p-3"
          style="background: linear-gradient( -135deg, transparent 20px, #161E1C 0);"
        >
          <h4
            class="mb-2 p-1"
            style="background: linear-gradient( -135deg, transparent 20px, #FA8322 0);"
          >
            Geo-magnetic field
          </h4>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Latitude:</span>
            <span class="status-text">{earthData.latitude}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Longitude:</span>
            <span class="status-text">{earthData.longitude}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Altitude:</span>
            <span class="status-text">{earthData.altitude}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Intensity:</span>
            <span class="status-text">{earthData.intensity}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Declination:</span>
            <span class="status-text">{earthData.declination}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Inclination:</span>
            <span class="status-text">{earthData.inclination}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">North:</span>
            <span class="status-text">{earthData.north}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">East:</span>
            <span class="status-text">{earthData.east}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Vertical:</span>
            <span class="status-text">{earthData.vertical}</span>
          </div>
          <div class="status-icon">
            <span class="square" />
            <span class="status-label">Horizontal:</span>
            <span class="status-text">{earthData.horizontal}</span>
          </div>
        </div>
      </div>
    </div>
  </div>

</main>

