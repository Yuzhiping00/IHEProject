<template>
  <v-app>
    <router-view/>
  </v-app>
</template>

<script lang="ts" setup>
import { useRoute } from 'vue-router';
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  /* text-align: center; */
  color: #2c3e50;
  margin-top: 10px;
}
</style>
