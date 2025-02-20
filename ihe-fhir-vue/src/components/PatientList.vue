<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'
import Patient from '@/models/Patient'

const router = useRouter()
const patientStore = usePatientStore()
const patients = ref<Patient[]>([])
const errorMessage = ref('')

const columns = [
    { text: "Last Name", align: "left", value: "familyName" },
    { text: "First Name", align: "left", value: "givenName" },
    { text: "Gender", align: "left", value: "gender" },
    { text: "Birth Date", align: "left", value: "birthDate" },
    { text: "Actions", align: "left", value: "actions" },
];

onMounted(async () => {
    const response = await patientService.query()
    if (response.status === 200) {
        patients.value = response.data.map(({familyName, givenName, gender, birthDate}) => ({
            familyName,
            givenName,
            gender,
            birthDate,
            actions:''
        }))
    } else {
        errorMessage.value = response?.statusText || "Failed to retrieve patients"
    }
})

const deletePatient = (patient: any) => {
    console.log("deleting a patient: ", patient)
}

const editPatient = (patient: any) => {
    console.log("editing a patient: ", patient)
}

</script>

<template>
    <v-data-table :columns="columns" :items="patients">
        <template v-slot:[`item.familyName`]="{ item }">
            <td class="text-left">{{ item.familyName }}</td> <!-- Left align for name -->
        </template>
        <template v-slot:[`item.givenName`]="{ item }">
            <td class="text-left">{{ item.givenName }}</td> <!-- Center align for age -->
        </template>
        <template v-slot:[`item.gender`]="{ item }">
            <td class="text-left">{{ item.gender }}</td> <!-- Right align for gender -->
        </template>
        <template v-slot:[`item.birthDate`]="{ item }">
            <td class="text-left">{{ item.birthDate }}</td> <!-- Right align for gender -->
        </template>
        <template v-slot:[`item.actions`]="{ item }">
            <td class="text-left">
                <v-btn color="primary" @click="editPatient(item)">
                    <v-icon>mdi-pencil</v-icon>
                </v-btn>
                <v-btn color="red" class="ma-2" @click="deletePatient(item)">
                    <v-icon>mdi-delete</v-icon>
                </v-btn>
            </td>
        </template>

    </v-data-table>
</template>

<style scoped></style>
