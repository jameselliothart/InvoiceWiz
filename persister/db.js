const mongoose = require("mongoose");

mongoose.connect("mongodb://mongodb:27017/invoices");

const nameSchema = new mongoose.Schema({
  id: String,
  to: String,
  amount: Number,
});

exports.Invoice = mongoose.model('Invoice', nameSchema);