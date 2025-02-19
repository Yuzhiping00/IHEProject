import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import * as components from "vuetify/components";
import * as directives from "vuetify/directives";
import "@mdi/font/css/materialdesignicons.css";
import router from "@/router"

const pinia = createPinia();
const vuetify = createVuetify({
    theme: {
      defaultTheme: "dark",
      themes: {
        light: {
          colors: {
            primary: "#1976D2",
            secondary: "#424242",
          },
        },
      },
    },
    components,
    directives,
  });

const app = createApp(App)
app.use(router)
app.use(pinia)
app.use(vuetify)
app.mount('#app')