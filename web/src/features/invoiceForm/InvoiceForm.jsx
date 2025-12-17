import { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setCurrentInvoiceId } from "./invoiceSlice";
import { setDownloadUrl as setDownloadUrlAction } from "./downloadUrlSlice";

function InvoiceForm() {
  const [invoiceTo, setInvoiceTo] = useState("");
  const [invoiceDetails, setInvoiceDetails] = useState("");
  const [invoiceAmount, setInvoiceAmount] = useState("");
  const [invoiceDate, setInvoiceDate] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const dispatch = useDispatch();
  const downloadUrl = useSelector((s) => s.downloadUrl.value);
  const [statusMessage, setStatusMessage] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setIsSubmitting(true);
    setStatusMessage("");
    dispatch(setDownloadUrlAction(null));

    const payload = {
      id:
        typeof crypto !== "undefined" && crypto.randomUUID
          ? crypto.randomUUID()
          : undefined,
      to: invoiceTo,
      amount: invoiceAmount ? parseFloat(invoiceAmount) : 0,
      details: invoiceDetails,
      invoiceDate: invoiceDate,
    };

    try {
      const res = await fetch("/api/invoices", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (res.ok) {
        setStatusMessage("Invoice request submitted.");
        // track current invoice id in store so SignalR can match
        if (payload.id) dispatch(setCurrentInvoiceId(payload.id));
        // Try to parse a returned location (if the API includes it)
        // try {
        //   const json = await res.json().catch(() => null);
        //   if (json && (json.location || json.locationUri || json.uri)) {
        //     setDownloadUrl(json.location || json.locationUri || json.uri);
        //   }
        // } catch {}
      } else {
        const text = await res.text().catch(() => "");
        setStatusMessage(`Request failed: ${res.status} ${text}`);
      }
    } catch (err) {
      setStatusMessage(`Error: ${err.message}`);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="tab-pane fade show active" id="form" role="tabpanel">
      <form onSubmit={handleSubmit}>
        <div className="mb-3">
          <label htmlFor="invoiceTo" className="form-label">
            To
          </label>
          <input
            type="text"
            className="form-control"
            name="invoiceTo"
            id="invoiceTo"
            value={invoiceTo}
            onChange={(e) => setInvoiceTo(e.target.value)}
            required
          />
        </div>
        <div className="mb-3">
          <label htmlFor="invoiceDetails" className="form-label">
            Details
          </label>
          <textarea
            className="form-control"
            name="invoiceDetails"
            id="invoiceDetails"
            value={invoiceDetails}
            onChange={(e) => setInvoiceDetails(e.target.value)}
            required
          ></textarea>
        </div>
        <div className="mb-3">
          <label htmlFor="invoiceAmount" className="form-label">
            Amount
          </label>
          <input
            type="number"
            min="0.01"
            step="0.01"
            className="form-control"
            name="invoiceAmount"
            id="invoiceAmount"
            value={invoiceAmount}
            onChange={(e) => setInvoiceAmount(e.target.value)}
          />
        </div>
        <div className="mb-3">
          <label htmlFor="invoiceDate" className="form-label">
            Invoice Date
          </label>
          <input
            type="date"
            className="form-control"
            name="invoiceDate"
            id="invoiceDate"
            value={invoiceDate}
            onChange={(e) => setInvoiceDate(e.target.value)}
            required
          />
        </div>

        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Generating..." : "Generate"}
        </button>
        <a
          id="buttonDownload"
          className={`btn btn-secondary ${downloadUrl ? "" : "disabled"}`}
          role="button"
          href={downloadUrl || "#"}
          target={downloadUrl ? "_blank" : undefined}
          rel={downloadUrl ? "noopener noreferrer" : undefined}
        >
          Download
        </a>

        {statusMessage && (
          <div className="mt-2">
            <small>{statusMessage}</small>
          </div>
        )}
      </form>
    </div>
  );
}

export default InvoiceForm;
