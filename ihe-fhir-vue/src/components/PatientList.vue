<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import Patient from '@/models/Patient'
import DeletePatientModal from './DeletePatientModal.vue'
import EditPatientModal from './EditPatientModal.vue'


const router = useRouter()
const existingPatients = ref<Patient[]>([])
const isLoading = ref(true)
const editPatient = ref()

// selected patient to for deletion or viewing
const selectedPatient = ref<Patient | null>(null)
const filteredPatients = ref<Patient[]>([])
const deleteDialog = ref(false)
const editDialog = ref(false)
const search = ref('')


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
        existingPatients.value = response.data.entry.map((e: any) => e.resource)
        console.log("patients list: ", existingPatients.value)
    } else {
        isLoading.value = false
    }
})

// show the delete confirmation modal
const clickedDelete = (patient: Patient) => {
    selectedPatient.value = patient
    deleteDialog.value = true
}

// handle delete action
const confirmDeletePatient = async () => {
    isLoading.value = true
    // remove the selected patient from db
    const response = await patientService.delete(selectedPatient.value?.id)
    if (response.status === 204) {
        isLoading.value = false
        filteredPatients.value = existingPatients.value.filter((p: any) =>
            p.id != selectedPatient?.value?.id
        )
        existingPatients.value = filteredPatients.value
        deleteDialog.value = false
        selectedPatient.value = null
    } else {
        isLoading.value = false
        router.push({ name: 'NotFound' })
    }
}

const clickedEdit = async (patient: any) => {
    editDialog.value = true

    //deep copy
    editPatient.value = JSON.parse(JSON.stringify(patient))

    //parse YYYY-MM-DD Manually
    const [year, month, day] = patient.birthDate?.split("-").map(Number)

    editPatient.value.birthDate = new Date(year, month - 1, day)
}

const handleUpdate = async (updatedPatient: any) => {

    if (updatedPatient.birthDate) {

        // parse date string into a date object
        const date = new Date(updatedPatient.birthDate)

        //convert to YYYY-MM-DD
        updatedPatient.birthDate = date.toISOString().split('T')[0]

    }

    isLoading.value = true

    const response = await patientService.put(updatedPatient.id, updatedPatient)

    if (response.status === 200) {
        isLoading.value = false
        existingPatients.value[existingPatients.value.findIndex(p => p.id === updatedPatient.id)] = response.data
        editDialog.value = false

    } else {
        isLoading.value = false
        router.push({ name: 'NotFound' })
    }
}

const cancelUpdateForm = () => {
    editDialog.value = false
}

const createPatient = () => {
    router.push({ name: "PatientCreate" })
}

</script>

<template>
    <v-container v-if="existingPatients && existingPatients.length > 0">
        <v-card title="Patients" flat class="text-left">
            <template v-slot:text>
                <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" class="mt-5"
                    variant="outlined" hide-details single-line></v-text-field>
            </template>
            <!-- search patients -->
            <v-data-table :headers="headers" :items="existingPatients" :search="search">
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
                        <v-btn color="primary" @click="clickedEdit(item)">
                            <v-icon>mdi-pencil</v-icon>
                        </v-btn>
                        <v-btn color="red" class="ma-2" @click="clickedDelete(item)">
                            <v-icon>mdi-delete</v-icon>
                        </v-btn>
                    </td>
                </template>
            </v-data-table>
        </v-card>

        <!-- Edit Patient Modal -->
        <edit-patient-modal v-if="editDialog" :show-modal="editDialog" :patient="editPatient"
            @cancel-update="cancelUpdateForm" @update-patient="handleUpdate" />

        <!-- Delete  Confirmation Modal -->
        <delete-patient-modal v-if="deleteDialog" :show-modal="deleteDialog" @cancelDelete="deleteDialog = false"
            @confirmDelete="confirmDeletePatient" :selectedFamilyName="selectedPatient?.name[0].family"
            :selectedGivenName="selectedPatient?.name[0].given[0]" />

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
