import React, { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { useDispatch, useSelector } from "react-redux";
import { setDownloadUrl } from "../invoiceForm/downloadUrlSlice";

// SignalR base path - rely on proxy or same origin /invoiceHub
const HUB_URL = "/invoiceHub";

export default function SignalRProvider({ children }) {
  const dispatch = useDispatch();
  const currentId = useSelector((s) => s.invoice.currentId);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveInvoice", (id, location) => {
      console.log(`ReceiveInvoice: ${id} : ${location}`);
      // If no current id is tracked, still set download if present
      if (!currentId || id === currentId) {
        dispatch(setDownloadUrl(location));
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
