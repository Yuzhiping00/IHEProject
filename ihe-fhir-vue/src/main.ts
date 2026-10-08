import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import vuetify from "./plugins/vuetify";
import router from "@/router"

const pinia = createPinia();
const app = createApp(App)
app.use(router)
app.use(pinia)
app.use(vuetify)
app.mount('#app')