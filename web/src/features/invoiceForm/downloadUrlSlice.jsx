
import { createSlice } from "@reduxjs/toolkit";

const downloadUrlSlice = createSlice({
  name: "downloadUrl",
  initialState: { value: null },
  reducers: {
    setDownloadUrl: (state, action) => {
      state.value = action.payload;
    },
    resetDownloadUrl: (state) => {
      state.value = null;
    },
  },
});

export const { setDownloadUrl, resetDownloadUrl } = downloadUrlSlice.actions;
export default downloadUrlSlice.reducer;