
// export interface Meta {
//     versionId?: string
//     lastUpdated?: string // ISO datetime
// }

// export interface Narrative {
//     status: string
//     div:string //XHTML content as string
// }

// export interface Identifier {
//     use?: string
//     label?: string
//     system?: string
//     value?: string
// }

// export interface ContactPoint {
//    system?: "phone" | "email"
//    value?: string
//    use?: "home" | "work" | "mobile"
// }


export default class Patient {
    resourceType: string = "Patient"
    name: HumanName[] = [{family:" ", given:[""]}]
    gender?: "male" | "female" | "other" | "unknown"
    birthDate?: string
    id?: string;
    // name: { family?: string; given?: string[] }[] = [ { family: "", given: [""] } ]
    // meta?: Meta
    // text?: Narrative
    // telecom?:ContactPoint[]
    // identifier?: Identifier[]
}

export class HumanName {
    family: string = ""
    given: string[] = [""]
}





