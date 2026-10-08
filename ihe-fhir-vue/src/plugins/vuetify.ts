import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import * as components from "vuetify/components";
import * as directives from "vuetify/directives";
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

// ADD THIS LINE AT THE BOTTOM:
export default vuetify; 