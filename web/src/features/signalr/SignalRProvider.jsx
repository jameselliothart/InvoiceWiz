import React, { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch, useSelector } from "react-redux";
import { setDownloadUrl } from "../invoiceForm/downloadUrlSlice";
import { fetchInvoices } from "../invoiceGrid/invoicesSlice";
import { resetCurrentInvoiceId } from "../invoiceForm/invoiceSlice";

// SignalR base path - rely on proxy or same origin /invoiceHub
const HUB_URL = "/invoiceHub";
const RTMethod = "InvoiceGenerated";

export default function SignalRProvider({ children }) {
  const dispatch = useDispatch();
  const currentId = useSelector((s) => s.invoice.currentId);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    connection.on(RTMethod, (id, location) => {
      console.log(`${RTMethod}: ${id} : ${location}`);
      // Only refresh when the event matches the currently tracked invoice (if set)
      if (!currentId || id === currentId) {
        const downloadUrl = `/api/invoices/${id}/download`;
        dispatch(setDownloadUrl(downloadUrl));
        // Refresh invoice list so UI reflects generated location
        dispatch(fetchInvoices());
        // Clear the tracked id so future events don't get dropped
        if (currentId) dispatch(resetCurrentInvoiceId());
      }
    });

    connection.start().catch((err) => {
      console.warn("SignalR connect failed:", err);
    });

    return () => {
      connection.stop().catch(() => {});
    };
  }, [dispatch, currentId]);

  return children || null;
}
