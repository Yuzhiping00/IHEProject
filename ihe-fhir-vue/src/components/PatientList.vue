<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import Patient from '@/models/Patient'
import DeletePatientModal from './DeletePatientModal.vue'
import EditPatientModal from './EditPatientModal.vue'
import { usePatientStore } from '@/stores/patientStore'


const router = useRouter()
const patientStore = usePatientStore()
const existingPatients = ref<Patient[]>([])
const isLoading = ref(true)

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
        existingPatients.value = response.data.entry?.map((e: any) => {
            //flatten FHIR -> Patient Model
            const p = e.resource
            return new Patient ({
                id:p.id,
                familyName: p.name?.[0]?.family || "",
                givenName: p.name?.[0]?.given?.[0] || "",
                gender: p.gender,
                birthDate: p.birthDate
            }) 

        }) ?? []
        
    } else {
        isLoading.value = false
    }
})

// show the delete confirmation modal
const clickedDelete = (patient: Patient) => {
    // push selected patient into store
    patientStore.patient = new Patient(patient)
    deleteDialog.value = true
}

// handle delete action
const confirmDeletePatient = async () => {
    isLoading.value = true
    // remove the selected patient from db
    const response = await patientService.delete(patientStore.patient?.id)
    if (response.status === 204) {
        isLoading.value = false
        //use splice to remove 1 element
        // const index = existingPatients.value.findIndex(p => p.id === selectedPatient.value?.id)
        // if(index !== -1) {
        //     existingPatients.value.splice(index,1)
        // }
        existingPatients.value = existingPatients.value.filter(p => p.id != patientStore.patient?.id)
        deleteDialog.value = false
        patientStore.clearPatient()
    } else {
        isLoading.value = false
        router.push({ name: 'NotFound' })
    }
}

const clickedEdit = async (patient: any) => {
    editDialog.value = true
    patientStore.patient = new Patient(patient) // shallow copy into store
}

const handleUpdate = async () => {
    //Build a new clean FHIR patient object
    const fhirPatient = patientStore.toFhir()

    isLoading.value = true

    const response = await patientService.put(fhirPatient.id,fhirPatient)

    if (response.status === 200) {
        isLoading.value = false
        patientStore.fromFhir(response.data)

        //update list in table
        const index = existingPatients.value.findIndex(p => p.id === patientStore.patient.id)

        if(index !== -1) {
            existingPatients.value[index] = new Patient(patientStore.patient)
        }

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
        <v-card title="Patients" flat>
            <template v-slot:text>
                <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
                    hide-details single-line></v-text-field>
            </template>

            <!-- search patients -->
            <v-data-table :headers="headers" :items="existingPatients" :search="search">
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
        <edit-patient-modal v-if="editDialog" :show-modal="editDialog" :patient="patientStore.patient"
            @cancel-update="cancelUpdateForm" @update-patient="handleUpdate" />

        <!-- Delete  Confirmation Modal -->
        <delete-patient-modal v-if="deleteDialog" :show-modal="deleteDialog" @cancelDelete="deleteDialog = false"
            @confirmDelete="confirmDeletePatient" :selectedFamilyName="patientStore.patient.familyName"
            :selectedGivenName="patientStore.patient.givenName" />

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
