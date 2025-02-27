function handleSubmit(event) {
    event.preventDefault();
    const amount = event.target.invoiceAmount.value;
    // fetch
    alert(amount);
}