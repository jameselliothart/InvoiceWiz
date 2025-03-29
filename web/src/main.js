import './assets/main.css';

import { createApp } from 'vue';
import App from './App.vue';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

// OpenTelemetry Setup
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';

const provider = new WebTracerProvider();
const exporter = new OTLPTraceExporter({
    url: 'http://jaeger:4317/v1/traces',
});
provider.addSpanProcessor(new BatchSpanProcessor(exporter));
provider.register();
new FetchInstrumentation({}).register();

createApp(App).mount('#app')
