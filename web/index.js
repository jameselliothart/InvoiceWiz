const BASE_URI = 'http://localhost:8080/api';
const URI_INVOICE = `${BASE_URI}/invoice`;
const URI_SIGNALR = `${BASE_URI}/live`;

async function handleSubmit(event) {
  event.preventDefault();
  const id = crypto.randomUUID();
  const msg = {
    id,
    to: event.target.invoiceTo.value,
    amount: Number(event.target.invoiceAmount.value),
  };
  try {
    const body = JSON.stringify(msg);
    console.log(`Sending ${URI_INVOICE}: ${body}`);
    const response = await fetch(URI_INVOICE, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body,
    });
    document.getElementById('invoiceId').value = id;
    if (!response.ok) {
      throw new Error(`Response status: ${response.status}`);
    }
  } catch (error) {
    console.error(error.message);
  }
}

async function getInvoices() {
  try {
    const response = await fetch(URI_INVOICE);
    if (!response.ok) {
      throw new Error(`Response status: ${response.status}`);
    }

    const json = await response.json();
    console.log(json);
    return json;
  } catch (error) {
    console.error(error.message);
  }
}

const connection = new signalR.HubConnectionBuilder()
  .withUrl(URI_SIGNALR)
  .configureLogging(signalR.LogLevel.Information)
  .build();

async function start() {
  try {
    await connection.start();
    console.log("SignalR Connected.");
  } catch (err) {
    console.log(err);
    setTimeout(start, 5000);
  }
};

connection.onclose(async () => {
  await start();
});

connection.on("ReceiveInvoice", (id, location) => {
  console.log(`ReceiveInvoice: ${id} : ${location}`);
  const currentId = document.getElementById('invoiceId').value;
  if (id !== currentId) {
    console.log(`ReceiveInvoice: id does not match current invoice id: ${currentId}`);
    return
  }
  const downloadButton = document.getElementById("buttonDownload");
  downloadButton.href = location;
  downloadButton.classList.remove('btn-secondary', 'disabled');
  downloadButton.classList.add('btn-success');

  Toastify({
    text: "Click here to download your invoice!",
    destination: location,
    duration: 3000,
    }).showToast();
});

start();