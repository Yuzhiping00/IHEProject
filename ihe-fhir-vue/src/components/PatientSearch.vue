<script setup>
import { ref, watch } from 'vue'
import patientService from "@/services/patientService.js"

const patientName = ref('')
const patients = ref([])
const loading = ref(false)
const patientNotFound = ref(false)
const avartar = ref("https://randomuser.me/api/portraits/women/8.jpg")

const rules = [
  value => value ? true : 'You must enter a user name'
]
const handleSearch = async () => {
  if (!patientName.value.trim()) {
    patients.value = []
    patientNotFound.value = true
    return
  }

  loading.value = true

  try {
    var response = await patientService.searchPatients(patientName.value)
    patients.value = response.data;
    console.log("patients = ", patients.value)
    loading.value = false
    patientNotFound.value = false

  } catch (error) {
    console.log('Error fetching patients : ', error)
    patients.value = []
  }
}

</script>

<template>
  <v-sheet class="mx-auto pb-5" width="1000">
    <v-form @submit.prevent>
      <v-text-field v-model="patientName" label="User Name" :rules="rules"></v-text-field>
      <v-btn type="submit" class="mt-2" @click.native="handleSearch">Submit</v-btn>

    </v-form>

  </v-sheet>
  <div>
    <v-alert class="mt-10" type="info" elevation="2" v-if="patientNotFound && patientName">No Patients Found</v-alert>
  </div>
  <v-sheet >
    <v-list lines="three" color="cyan-lighten-1">
      <v-list-item v-for="patient in patients" :key="patient.id" :title="patient.name"
        :subtitle="patient.birthDate"
        :prepend-avatar="avartar">
      </v-list-item>
    </v-list>
  </v-sheet>
</template>

<style scoped></style>
