const URI_INVOICE = 'http://localhost:8080/api/invoice';

async function handleSubmit(event) {
  event.preventDefault();
  const msg = {
    id: crypto.randomUUID(),
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