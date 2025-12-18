import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchInvoices } from "./invoicesSlice";

export default function InvoiceGrid() {
  const dispatch = useDispatch();
  const { items, loading, error } = useSelector((s) => s.invoices);

  useEffect(() => {
    dispatch(fetchInvoices());
  }, [dispatch]);

  return (
    <div className="mt-4">
      <h2>Past Invoices</h2>
      {loading && <div>Loading...</div>}
      {error && <div className="text-danger">Error: {error}</div>}
      {!loading && !error && (
        <div className="table-responsive">
          <table className="table table-striped">
            <thead>
              <tr>
                <th>To</th>
                <th>Amount</th>
                <th>Created</th>
                <th>Invoice Date</th>
                <th>Download</th>
              </tr>
            </thead>
            <tbody>
              {items && items.length > 0 ? (
                items.map((row) => (
                  <tr key={row.id}>
                    <td>{row.to}</td>
                    <td>{row.amount}</td>
                    <td>{new Date(row.createdDate).toLocaleString()}</td>
                    <td>{row.invoiceDate}</td>
                    <td>
                      {
                        <a
                          href={`/api/invoices/${row.id}/download`}
                          className="btn btn-sm btn-outline-primary"
                        >
                          Download
                        </a>
                      }
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5}>No invoices found</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
