<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import Patient from '@/models/Patient'
import DeletePatientModal from './DeletePatientModal.vue'

const router = useRouter()
const retrievedPatients = ref<Patient[]>([])
const displayedPatients = ref<any[]>()
const isLoading = ref(true)


const headers = [
    { title: "Last Name", align: "start", key: "familyName" },
    { title: "First Name", align: "start", key: "givenName" },
    { title: "Gender", align: "start", key: "gender" },
    { title: "Birth Date", align: "start", key: "birthDate" },
    { title: "Actions", align: "start", key: "actions" },
];

onMounted(async () => {
    isLoading.value = true
    const response = await patientService.query()
    if (response.status === 200) {
        isLoading.value = false
        retrievedPatients.value = response.data.entry.map((e:any) => e.resource)
    } else {
        isLoading.value = false
    }
})

const createPatient = () => {
    router.push({ name: "PatientCreate" })
}

</script>

<template>
    <v-container v-if="retrievedPatients && retrievedPatients.length > 0">
        <v-card title="Patients" flat class="text-left">
            <!-- search patients -->
            <v-data-table :headers="headers" :items="retrievedPatients">
                <template v-slot:[`item.familyName`]="{ item }">
                    <td class="text-left">{{ item.name[0].family }}</td> <!-- Left align for name -->
                </template>
                <template v-slot:[`item.givenName`]="{ item }">
                    <td class="text-left">{{ item.name[0].given[0] }}</td> <!-- Center align for age -->
                </template>
                <template v-slot:[`item.gender`]="{ item }">
                    <td class="text-left">{{ item.gender }}</td> <!-- Right align for gender -->
                </template>
                <template v-slot:[`item.birthDate`]="{ item }">
                    <td class="text-left">{{ item.birthDate }}</td>
                    <!-- Right align for gender -->
                </template>
                <template v-slot:[`item.actions`]="{ item }">
                    <td class="text-left">
                        <v-btn color="primary" >
                            <v-icon>mdi-pencil</v-icon>
                        </v-btn>
                        <v-btn color="red" class="ma-2">
                            <v-icon>mdi-delete</v-icon>
                        </v-btn>
                    </td>
                </template>
            </v-data-table>
        </v-card>

    </v-container>

    <v-container v-else>
        <v-card>
            <v-alert type="warning" variant="outlined" prominent>
                <p class="font-weight-bold">No patients found. Please create a new patient below to get started.</p>
            </v-alert>
            <br />
            <v-btn @click="createPatient" color="success" rounded="xl" variant="elevated" class="mb-6">
                Create
            </v-btn>
        </v-card>

    </v-container>
</template>

<style scoped></style>
