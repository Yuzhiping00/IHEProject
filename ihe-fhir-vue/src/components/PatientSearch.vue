<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'

const router = useRouter()
const patientStore = usePatientStore()
const patientName = ref('')
const patients = ref([])
const loading = ref(false)
const patientNotFound = ref(false)
const avartar = ref("https://randomuser.me/api/portraits/women/8.jpg")

const rules = [
  (value: any) => value ? true : 'You must enter a user name'
]
const handleSearch = async () => {
  if (!patientName.value.trim()) {
    patients.value = []
    patientNotFound.value = true
    return
  }

  loading.value = true

  try {
    var response = await patientService.get(1)
    patientStore.patient = response.data;
    console.log("The patient retrieved = ", patientStore.patient)
    loading.value = false
    patientNotFound.value = false

  } catch (error) {
    patients.value = []
    router.push({ name: 'NotFound' })
  }
}

</script>

<template>
  <v-sheet class="mx-auto pb-5" width="1000">
    <v-form @submit.prevent>
      <v-text-field v-model="patientName" label="User ID" :rules="rules"></v-text-field>
      <v-btn type="submit" class="mt-2" @click.native="handleSearch">Search Patient</v-btn>
    </v-form>
  </v-sheet>
  <div>
    <v-alert class="mt-10" type="info" elevation="2" v-if="patientNotFound && patientName">No Patients Found</v-alert>
  </div>
  <p>{{ patientStore.patient.givenName}}</p>
  <!-- <v-sheet>
    <v-list lines="three" color="cyan-lighten-1">
      <v-list-item v-for="patient in patients" :key="patient.id" :title="patient.name" :subtitle="patient.birthDate"
        :prepend-avatar="avartar">
      </v-list-item>
    </v-list>
  </v-sheet> -->
</template>

<style scoped></style>
