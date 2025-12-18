import React, { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch, useSelector } from "react-redux";
import { setDownloadUrl } from "../invoiceForm/downloadUrlSlice";
import { fetchInvoices } from "../invoiceGrid/invoicesSlice";
import { resetCurrentInvoiceId } from "../invoiceForm/invoiceSlice";
import Toastify from "toastify-js";
import "toastify-js/src/toastify.css";

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
      try {
        console.log(`${RTMethod}: ${id} : ${location}`);
        // Only refresh when the event matches the currently tracked invoice (if set)
        if (!currentId || id === currentId) {
          const downloadUrl = `/api/invoices/${id}/download`;
          dispatch(setDownloadUrl(downloadUrl));
          // Show a toast notification with a clickable download link
          Toastify({
            text: "Click here to download your invoice!",
            destination: downloadUrl,
            duration: 5000,
          }).showToast();
          // Refresh invoice list so UI reflects generated location
          dispatch(fetchInvoices());
          // Clear the tracked id so future events don't get dropped
          if (currentId) dispatch(resetCurrentInvoiceId());
        }
      } catch (err) {
        console.error("Error handling SignalR message:", err);
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
