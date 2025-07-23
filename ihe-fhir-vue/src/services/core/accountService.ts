import type { AxiosResponse } from "axios";
import AxiosService from "@/services/core/axiosService";
import ContentResult from "@/models/ContenResult";

/**
 * Represents an User service.
 */
class AccountService extends AxiosService {
  /**
   * Gets or set the path of the API for the service.
   */
  protected rootPath: string = "api/account";

  /**
   * Gets or set the path of the API for the service.
   */
  basePath: string = "account";


  /**
   * Logs a user in given they provide the correct email and password
   */
  loginAsync(data: object): Promise<AxiosResponse<any>> {
    return this.axios
      .post<any>(`${this.rootPath}/login`, data)
      .catch(this.genericErrorHandler);
  }

  async resetPasswordAsync(data:object): Promise<AxiosResponse<any>> {
    try {
      return await this.axios
        .post<any>(`${this.rootPath}/reset-password`, data);
    } catch (error) {
      return this.genericErrorHandler(error);
    }
  }
}

export default new AccountService();
